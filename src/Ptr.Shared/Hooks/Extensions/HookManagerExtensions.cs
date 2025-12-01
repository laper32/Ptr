using Ptr.Shared.Hooks.Managers;
using Ptr.Shared.Hooks.Params;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;

namespace Ptr.Shared.Hooks.Extensions;

public static class HookManagerExtensions
{
    extension(IHookManager hookManager)
    {
        public IHookType<IHandleDropWeaponHookParams, EmptyHookReturn> HandleDropWeapon =>
            INativeHookManager.Instance.HandleDropWeapon;

        public IHookType<IHandleCommandBuyHookParams, EmptyHookReturn> HandleCommandBuy =>
            INativeHookManager.Instance.HandleCommandBuy;
    }
}