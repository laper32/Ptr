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
        ///     获取实体的Controller可见状态
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="controller">Player Controller</param>
        /// <param name="channel">Channel, -1为读取全局状态</param>
        public bool GetEntityState(IBaseEntity entity, IPlayerController controller,
            int channel = -1)
        {
            return self.GetEntityState(entity.Index, controller.Index, channel);
        }

        /// <summary>
        ///     设置实体的Controller可见状态
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="controller">Player Controller</param>
        /// <param name="transmit">是否可见</param>
        /// <param name="channel">Channel</param>
        public bool SetEntityState(IBaseEntity entity, IPlayerController controller,
            bool transmit, int channel)
        {
            return self.SetEntityState(entity.Index, controller.Index, transmit, channel);
        }

        /// <summary>
        ///     获取实体是否被Block
        /// </summary>
        public bool GetEntityBlock(IBaseEntity entity)
        {
            return self.GetEntityBlock(entity.Index);
        }

        /// <summary>
        ///     设置实体Block State
        /// </summary>
        public bool SetEntityBlock(IBaseEntity entity, bool state)
        {
            return self.SetEntityBlock(entity.Index, state);
        }

        /// <summary>
        ///     获取Hook中的实体Owner
        /// </summary>
        /// <returns>-2 = NoHook | -1 = Null | other = Entity Index</returns>
        public int GetEntityOwner(IBaseEntity entity)
        {
            return self.GetEntityOwner(entity.Index);
        }

        /// <summary>
        ///     设置Hook中的实体Owner
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="owner">Owner实体的</param>
        public bool SetEntityOwner(IBaseEntity entity, IBaseEntity owner)
        {
            return self.SetEntityOwner(entity.Index, owner.Index);
        }

        /// <summary>
        ///     TempEnt的状态
        /// </summary>
        /// <param name="type">TE类型</param>
        /// <param name="client">IGameClient</param>
        public bool GetTempEntState(BlockTempEntType type, IGameClient client)
        {
            return self.GetTempEntState(type, client.Slot);
        }

        /// <summary>
        ///     设置TempEnt的状态
        /// </summary>
        /// <param name="type">TE类型</param>
        /// <param name="client">IGameClient</param>
        /// <param name="state">可见状态</param>
        public void SetTempEntState(BlockTempEntType type, IGameClient client,
            bool state)
        {
            self.SetTempEntState(type, client.Slot, state);
        }

        /// <summary>
        ///     重置接受者的所有实体状态
        /// </summary>
        /// <param name="receiver">receiver controller</param>
        public void ClearReceiverState(IPlayerController receiver)
        {
            self.ClearReceiverState(receiver.Index);
        }
    }
}