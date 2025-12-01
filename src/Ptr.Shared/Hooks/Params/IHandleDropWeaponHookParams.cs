using Sharp.Shared.GameObjects;
using Sharp.Shared.HookParams;

namespace Ptr.Shared.Hooks.Params;

public interface IHandleDropWeaponHookParams : IPlayerPawnFunctionParams, IFunctionParams
{
    IWeaponService Service { get; }
}