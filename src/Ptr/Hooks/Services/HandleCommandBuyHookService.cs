// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable InvertIf

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Ptr.Shared.Hooks.Abstractions;
using Ptr.Shared.Hooks.Params;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.GameObjects;
using Sharp.Shared.HookParams;
using Sharp.Shared.Hooks;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace Ptr.Hooks.Services;

internal class HandleCommandBuyHookParams(
    IGameClient client,
    IPlayerController controller,
    IPlayerPawn pawn,
    uint itemSlot) : IHandleCommandBuyHookParams
{
    public IGameClient Client { get; init; } = client;
    public IPlayerController Controller { get; init; } = controller;
    public IPlayerPawn Pawn { get; init; } = pawn;

    public uint ItemSlot { get; init; } = itemSlot;

    public bool IsDisposed { get; private set; }

    public void MarkAsDisposed()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
    }

    public void CheckDisposed()
    {
        if (IsDisposed)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
        }
    }
}

internal unsafe class HandleCommandBuyHookService :
    AbstractDetourHook<HandleCommandBuyHookService>,
    IHookType<IHandleCommandBuyHookParams, EmptyHookReturn>
{
    private static nint _trampoline = nint.Zero;
    private readonly ISharedSystem _bridge;
    private readonly ILogger<HandleCommandBuyHookService> _logger;
    private readonly List<PostHookInfo> _postHooks = [];

    private readonly List<PreHookInfo> _preHooks = [];

    public HandleCommandBuyHookService(IModSharpModule module, string name,
        ISharedSystem bridge,
        ILogger<HandleCommandBuyHookService> logger) : base(module, name, bridge, logger)
    {
        Instance = this;
        _bridge = bridge;
        _logger = logger;
    }

    public static HandleCommandBuyHookService Instance { get; private set; } = null!;

    protected virtual bool AllowPre => true;
    protected virtual bool AllowPost => true;

    // AbstractHookService functionality
    private record PreHookInfo(
        Func<IHandleCommandBuyHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> Callback,
        int Priority);

    private record PostHookInfo(
        Action<IHandleCommandBuyHookParams, HookReturnValue<EmptyHookReturn>> Callback,
        int Priority);

    #region AbstractDetourHook Implementation

    protected override void Prepare(IDetourHook hook)
    {
        hook.Prepare(Name, (nint)(delegate* unmanaged<nint, uint, int>)&Hook);
    }

    protected override void InternalShutdown()
    {
        _preHooks.Clear();
        _postHooks.Clear();
        _trampoline = nint.Zero;
    }

    protected override void InternalPostInstall(IntPtr trampoline)
    {
        _trampoline = trampoline;
    }


    [UnmanagedCallersOnly]
    private static int Hook(nint pService, uint nItemSlot)
    {
        return Instance.HookInternal(pService, nItemSlot);
    }

    private int HookInternal(nint pService, uint nItemSlot)
    {
        var call = (delegate* unmanaged<nint, uint, int>)_trampoline;

        if (_bridge.GetModSharp().CreateNativeObject<IWeaponService>(pService) is not { } buyService)
        {
            return call(pService, nItemSlot);
        }

        if (buyService.GetPlayer() is not IPlayerPawn pawn)
        {
            return call(pService, nItemSlot);
        }

        if (pawn.GetController() is not { } controller)
        {
            return call(pService, nItemSlot);
        }

        if (controller.GetGameClient() is not { } client)
        {
            return call(pService, nItemSlot);
        }

        var hookParams = new HandleCommandBuyHookParams(client, controller, pawn, nItemSlot);

        // Invoke pre-hooks
        var preResult = InvokeHookPre(hookParams);
        switch (preResult.Action)
        {
            case EHookAction.SkipCallReturnOverride:
            {
                InvokeHookPost(hookParams, preResult);
                return (int)EBuyResult.PlayerCantBuy;
            }
            case EHookAction.ChangeParamReturnDefault or EHookAction.ChangeParamReturnOverride
                or EHookAction.IgnoreParamReturnOverride:
            {
                _logger.LogError(
                    "ChangeParamReturnDefault, ChangeParamReturnOverride, IgnoreParamReturnOverride is not supported.");
                return (int)EBuyResult.PlayerCantBuy;
            }
            default:
            {
                // Call original function
                var ret = call(pService, nItemSlot);

                // Invoke post-hooks
                var postResult = new HookReturnValue<EmptyHookReturn>(EHookAction.Ignored);
                InvokeHookPost(hookParams, postResult);
                return ret;
            }
        }
    }

    #endregion

    #region AbstractHookService Implementation

    public void InstallHookPre(
        Func<IHandleCommandBuyHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> pre,
        int priority)
    {
        if (!AllowPre)
        {
            throw new InvalidOperationException($"Pre Hook of {GetType().Name} is not supported");
        }

        if (_preHooks.Any(x => x.Callback.Equals(pre)))
        {
            return;
        }

        _preHooks.Add(new PreHookInfo(pre, priority));
        _preHooks.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }

    public void InstallHookPost(Action<IHandleCommandBuyHookParams, HookReturnValue<EmptyHookReturn>> post,
        int priority)
    {
        if (!AllowPost)
        {
            throw new InvalidOperationException($"Post Hook of {GetType().Name} is not supported");
        }

        if (_postHooks.Any(x => x.Callback.Equals(post)))
        {
            return;
        }

        _postHooks.Add(new PostHookInfo(post, priority));
        _postHooks.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }

    public void RemoveHookPre(
        Func<IHandleCommandBuyHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> pre)
    {
        if (_preHooks.Find(x => x.Callback.Equals(pre)) is not { } hook)
        {
            throw new EntryPointNotFoundException("The hook callback was not found");
        }

        _preHooks.Remove(hook);
    }

    public void RemoveHookPost(Action<IHandleCommandBuyHookParams, HookReturnValue<EmptyHookReturn>> post)
    {
        if (_postHooks.Find(x => x.Callback.Equals(post)) is not { } hook)
        {
            throw new EntryPointNotFoundException("The hook callback was not found");
        }

        _postHooks.Remove(hook);
    }

    public HookReturnValue<EmptyHookReturn> InvokeHookPre(IHandleCommandBuyHookParams @params)
    {
        if (_preHooks.Count == 0)
        {
            return new HookReturnValue<EmptyHookReturn>(EHookAction.Ignored);
        }

        var action = EHookAction.Ignored;
        var result = new EmptyHookReturn();

        for (var i = 0; i < _preHooks.Count; i++)
        {
            try
            {
                var hook = _preHooks[i];
                var call = hook.Callback(@params, new HookReturnValue<EmptyHookReturn>(action, result));

                if (call.Action > action)
                {
                    action = call.Action;
                    result = call.ReturnValue;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An error occurred while calling pre hook of {type}",
                    GetType().Name);
            }
        }

        return new HookReturnValue<EmptyHookReturn>(action, result);
    }

    public void InvokeHookPost(IHandleCommandBuyHookParams @params, HookReturnValue<EmptyHookReturn> @return)
    {
        if (_postHooks.Count == 0)
        {
            return;
        }

        for (var i = 0; i < _postHooks.Count; i++)
        {
            try
            {
                _postHooks[i].Callback(@params, @return);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while calling post hook of {type}", GetType().Name);
            }
        }
    }

    public bool IsHookInvokeRequired(bool isPost)
    {
        return !isPost ? _preHooks.Count != 0 : _postHooks.Count != 0;
    }

    #endregion
}