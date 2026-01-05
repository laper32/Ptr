using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace Ptr.Shared.Extensions;

public static class TransmitManagerExtensions
{
    /// <param name="self"></param>
    extension(ITransmitManager self)
    {
        /// <summary>
        ///     Get the entity's visibility state for a Controller
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="controller">Player Controller</param>
        /// <param name="channel">Channel, -1 to read global state</param>
        public bool GetEntityState(IBaseEntity entity, IPlayerController controller,
            int channel = -1)
        {
            return self.GetEntityState(entity.Index, controller.Index, channel);
        }

        /// <summary>
        ///     Set the entity's visibility state for a Controller
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="controller">Player Controller</param>
        /// <param name="transmit">Whether visible</param>
        /// <param name="channel">Channel</param>
        public bool SetEntityState(IBaseEntity entity, IPlayerController controller,
            bool transmit, int channel)
        {
            return self.SetEntityState(entity.Index, controller.Index, transmit, channel);
        }

        /// <summary>
        ///     Get whether the entity is blocked
        /// </summary>
        public bool GetEntityBlock(IBaseEntity entity)
        {
            return self.GetEntityBlock(entity.Index);
        }

        /// <summary>
        ///     Set entity block state
        /// </summary>
        public bool SetEntityBlock(IBaseEntity entity, bool state)
        {
            return self.SetEntityBlock(entity.Index, state);
        }

        /// <summary>
        ///     Get the entity's owner in the hook
        /// </summary>
        /// <returns>-2 = NoHook | -1 = Null | other = Entity Index</returns>
        public int GetEntityOwner(IBaseEntity entity)
        {
            return self.GetEntityOwner(entity.Index);
        }

        /// <summary>
        ///     Set the entity's owner in the hook
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="owner">Owner entity</param>
        public bool SetEntityOwner(IBaseEntity entity, IBaseEntity owner)
        {
            return self.SetEntityOwner(entity.Index, owner.Index);
        }

        /// <summary>
        ///     Get TempEnt state
        /// </summary>
        /// <param name="type">TE type</param>
        /// <param name="client">IGameClient</param>
        public bool GetTempEntState(BlockTempEntType type, IGameClient client)
        {
            return self.GetTempEntState(type, client.Slot);
        }

        /// <summary>
        ///     Set TempEnt state
        /// </summary>
        /// <param name="type">TE type</param>
        /// <param name="client">IGameClient</param>
        /// <param name="state">Visibility state</param>
        public void SetTempEntState(BlockTempEntType type, IGameClient client,
            bool state)
        {
            self.SetTempEntState(type, client.Slot, state);
        }

        /// <summary>
        ///     Reset all entity states for the receiver
        /// </summary>
        /// <param name="receiver">receiver controller</param>
        public void ClearReceiverState(IPlayerController receiver)
        {
            self.ClearReceiverState(receiver.Index);
        }
    }
}