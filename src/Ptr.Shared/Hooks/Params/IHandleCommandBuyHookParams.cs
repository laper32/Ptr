using Sharp.Shared.HookParams;

namespace Ptr.Shared.Hooks.Params;

public enum EBuyResult
{
    Bought,
    AlreadyHave,
    CantAfford,
    PlayerCantBuy, // not in the buy zone, is the VIP, is past the timelimit, etc
    NotAllowed, // weapon is restricted by VIP mode, team, etc
    InvalidItem
}

public interface IHandleCommandBuyHookParams : IPlayerPawnFunctionParams, IFunctionParams
{
    uint ItemSlot { get; }
}