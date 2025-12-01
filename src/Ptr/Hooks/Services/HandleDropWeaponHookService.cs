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

internal class HandleDropWeaponHookParams(
    IGameClient client,
    IPlayerController controller,
    IPlayerPawn pawn,
    IBaseWeapon? weapon,
    IWeaponService service) : IHandleDropWeaponHookParams
{
    public IBaseWeapon? Weapon => weapon ?? Service.ActiveWeapon;

    public IGameClient Client { get; init; } = client;
    public IPlayerController Controller { get; init; } = controller;
    public IPlayerPawn Pawn { get; init; } = pawn;

    public IWeaponService Service { get; init; } = service;


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

internal unsafe class HandleDropWeaponHookService :
    AbstractDetourHook<HandleDropWeaponHookService>,
    IHookType<IHandleDropWeaponHookParams, EmptyHookReturn>
{
    private static nint _trampoline = nint.Zero;
    private readonly ISharedSystem _bridge;
    private readonly ILogger<HandleDropWeaponHookService> _logger;
    private readonly List<PostHookInfo> _postHooks = [];

    private readonly List<PreHookInfo> _preHooks = [];

    public HandleDropWeaponHookService(IModSharpModule module, string name, ISharedSystem bridge,
        ILogger<HandleDropWeaponHookService> logger) :
        base(module, name, bridge, logger)
    {
        Instance = this;
        _bridge = bridge;
        _logger = logger;
    }

    public static HandleDropWeaponHookService Instance { get; private set; } = null!;

    protected virtual bool AllowPre => true;
    protected virtual bool AllowPost => true;

    // AbstractHookService functionality
    private record PreHookInfo(
        Func<IHandleDropWeaponHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> Callback,
        int Priority);

    private record PostHookInfo(
        Action<IHandleDropWeaponHookParams, HookReturnValue<EmptyHookReturn>> Callback,
        int Priority);

    #region AbstractVirtualHook Implementation

    protected override void Prepare(IDetourHook hook)
    {
        hook.Prepare(Name, (nint)(delegate* unmanaged<nint, nint, bool, bool>)&Hook);
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
    private static bool Hook(nint pService, nint pWeapon, bool swap)
    {
        return Instance.HookInternal(pService, pWeapon, swap);
    }


    private bool HookInternal(nint pService, nint pWeapon, bool swap)
    {
        var call = (delegate* unmanaged<nint, nint, bool, bool>)_trampoline;

        if (_bridge.GetModSharp().CreateNativeObject<IWeaponService>(pService) is not { } weaponService)
        {
            return call(pService, pWeapon, swap);
        }

        var weapon = _bridge.GetModSharp().CreateNativeObject<IBaseWeapon>(pWeapon);


        if (weaponService.GetPlayer() is not IPlayerPawn pawn)
        {
            return call(pService, pWeapon, swap);
        }

        if (pawn.GetController() is not { } controller)
        {
            return call(pService, pWeapon, swap);
        }

        if (_bridge.GetClientManager().GetGameClient(controller.SteamId) is not { } client)
        {
            return call(pService, pWeapon, swap);
        }

        var hookParams = new HandleDropWeaponHookParams(client, controller, pawn, weapon, weaponService);

        // Invoke pre-hooks
        var preResult = InvokeHookPre(hookParams);
        switch (preResult.Action)
        {
            case EHookAction.SkipCallReturnOverride:
            {
                InvokeHookPost(hookParams, preResult);
                return false;
            }
            case EHookAction.ChangeParamReturnDefault or EHookAction.ChangeParamReturnOverride
                or EHookAction.IgnoreParamReturnOverride:
            {
                _logger.LogError(
                    "ChangeParamReturnDefault, ChangeParamReturnOverride, IgnoreParamReturnOverride is not supported.");
                return false;
            }
            default:
            {
                // Call original function
                var ret = call(pService, pWeapon, swap);

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
        Func<IHandleDropWeaponHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> pre,
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

    public void InstallHookPost(Action<IHandleDropWeaponHookParams, HookReturnValue<EmptyHookReturn>> post,
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
        Func<IHandleDropWeaponHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> pre)
    {
        if (_preHooks.Find(x => x.Callback.Equals(pre)) is not { } hook)
        {
            _logger.LogDebug("Hook callback was not found during removal - it may have already been removed");
            return;
        }

        _preHooks.Remove(hook);
    }

    public void RemoveHookPost(Action<IHandleDropWeaponHookParams, HookReturnValue<EmptyHookReturn>> post)
    {
        if (_postHooks.Find(x => x.Callback.Equals(post)) is not { } hook)
        {
            _logger.LogDebug("Hook callback was not found during removal - it may have already been removed");
            return;
        }

        _postHooks.Remove(hook);
    }

    public HookReturnValue<EmptyHookReturn> InvokeHookPre(IHandleDropWeaponHookParams @params)
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

    public void InvokeHookPost(IHandleDropWeaponHookParams @params, HookReturnValue<EmptyHookReturn> @return)
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