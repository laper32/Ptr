// Warning: This file is only used for GPT generating. DO NOT EDIT!

// ReSharper disable ForCanBeConvertedToForeach

using Microsoft.Extensions.Logging;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;

namespace Ptr.Shared.Hooks.Abstractions;

#region Hook Impl

internal interface IHookService<in TParams, THookReturn> where TParams : class, IFunctionParams
{
    /// <summary>
    ///     Broadcast to other subscribers
    /// </summary>
    /// <typeparam name="TParams">Hook parameters</typeparam>
    /// <typeparam name="THookReturn">Hook return value (if void, just use int)</typeparam>
    /// <param name="params"></param>
    /// <returns>Encapsulates the hook action and hook return value</returns>
    HookReturnValue<THookReturn> InvokeHookPre(TParams @params);

    void InvokeHookPost(TParams @params, HookReturnValue<THookReturn> @return);

    bool IsHookInvokeRequired(bool isPost);
}

internal abstract class AbstractHookService<TParams, THookReturn, TClass>(ILogger<TClass> logger) :
    IHookService<TParams, THookReturn>,
    IHookType<TParams, THookReturn>
    where TParams : class, IFunctionParams
    where TClass : AbstractHookService<TParams, THookReturn, TClass>
{
    private readonly ILogger _logger = logger;
    private readonly List<PostHookInfo> _postHooks = [];

    private readonly List<PreHookInfo> _preHooks = [];

    protected abstract bool AllowPre { get; }
    protected abstract bool AllowPost { get; }

    public HookReturnValue<THookReturn> InvokeHookPre(TParams @params)
    {
        if (_preHooks.Count == 0)
        {
            return new HookReturnValue<THookReturn>(EHookAction.Ignored);
        }

        var action = EHookAction.Ignored;
        THookReturn? result = default;

        for (var i = 0; i < _preHooks.Count; i++)
        {
            try
            {
                var hook = _preHooks[i];
                var call = hook.Callback(@params, new HookReturnValue<THookReturn>(action, result));

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

        return new HookReturnValue<THookReturn>(action, result);
    }

    public void InvokeHookPost(TParams @params, HookReturnValue<THookReturn> @return)
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

    public void InstallHookPre(Func<TParams, HookReturnValue<THookReturn>, HookReturnValue<THookReturn>> pre,
        int priority)
    {
        if (!AllowPre)
        {
            throw new InvalidOperationException($"Pre Hook of {GetType().Name} is not support");
        }

        if (_preHooks.Any(x => x.Callback.Equals(pre)))
        {
            return;
        }

        _preHooks.Add(new PreHookInfo(pre, priority));
        _preHooks.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }

    public void InstallHookPost(Action<TParams, HookReturnValue<THookReturn>> post, int priority)
    {
        if (!AllowPost)
        {
            throw new InvalidOperationException($"Post Hook of {GetType().Name} is not support");
        }

        if (_postHooks.Any(x => x.Equals(post)))
        {
            return;
        }

        _postHooks.Add(new PostHookInfo(post, priority));
        _postHooks.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }

    public void RemoveHookPre(Func<TParams, HookReturnValue<THookReturn>, HookReturnValue<THookReturn>> pre)
    {
        if (_preHooks.Find(x => x.Callback.Equals(pre)) is not { } hook)
        {
            _logger.LogDebug("Hook callback was not found during removal - it may have already been removed");
            return;
        }

        _preHooks.Remove(hook);
    }

    public void RemoveHookPost(Action<TParams, HookReturnValue<THookReturn>> post)
    {
        if (_postHooks.Find(x => x.Callback.Equals(post)) is not { } hook)
        {
            _logger.LogDebug("Hook callback was not found during removal - it may have already been removed");
            return;
        }

        _postHooks.Remove(hook);
    }

    private record PreHookInfo(
        Func<TParams, HookReturnValue<THookReturn>, HookReturnValue<THookReturn>> Callback,
        int Priority);

    private record PostHookInfo(Action<TParams, HookReturnValue<THookReturn>> Callback, int Priority);
}

#endregion

#region Forward Impl

internal interface IForwardService<in TParams> where TParams : class, IFunctionParams
{
    void InvokeForward(TParams @params);

    bool IsForwardInvokeRequired();
}

// Used for GPT generating, DO NOT EDIT!
internal abstract class AbstractForwardService<TParams, TClass>(ILogger<TClass> logger)
    : IForwardService<TParams>, IForwardType<TParams>
    where TParams : class, IFunctionParams
    where TClass : AbstractForwardService<TParams, TClass>
{
    private readonly List<HookInfo> _hooks = [];

    public void InvokeForward(TParams @params)
    {
        if (_hooks.Count == 0)
        {
            return;
        }

        for (var i = 0; i < _hooks.Count; i++)
        {
            try
            {
                _hooks[i].Callback(@params);
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while calling forward of {type}", GetType().Name);
            }
        }
    }

    public bool IsForwardInvokeRequired()
    {
        return _hooks.Count > 0;
    }

    public void InstallForward(Action<TParams> func, int priority)
    {
        if (_hooks.Any(x => x.Callback.Equals(func)))
        {
            return;
        }

        _hooks.Add(new HookInfo(func, priority));
        _hooks.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }

    public void RemoveForward(Action<TParams> func)
    {
        if (_hooks.Find(x => x.Callback.Equals(func)) is not { } hook)
        {
            logger.LogDebug("Forward callback was not found during removal - it may have already been removed");
            return;
        }

        _hooks.Remove(hook);
    }

    private record HookInfo(Action<TParams> Callback, int Priority);
}

#endregion