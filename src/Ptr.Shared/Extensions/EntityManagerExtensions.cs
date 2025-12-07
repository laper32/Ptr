using Ptr.Shared.Bridge;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Units;

namespace Ptr.Shared.Extensions;

public static class EntityManagerExtensions
{
    extension(IEntityManager self)
    {
        /// <summary>
        ///     通过PlayerSlot查找PlayerController
        /// </summary>
        public IPlayerController? GetPlayerController(PlayerSlot slot)
        {
            return self.FindPlayerControllerBySlot(slot);
        }

        /// <summary>
        ///     通过IGameClient查找PlayerController
        /// </summary>
        public IPlayerController? GetPlayerController(IGameClient client)
        {
            return self.GetPlayerController(client.Slot);
        }

        public IPlayerController? GetPlayerController(SteamID steamId)
        {
            return InterfaceBridge.Instance.ClientManager.GetGameClient(steamId) is not { } client
                ? null
                : self.GetPlayerController(client);
        }

        public IBasePlayerPawn? GetPlayerPawn(IGameClient client)
        {
            return self.FindPlayerPawnBySlot(client.Slot);
        }

        public IEnumerable<IPlayerPawn> GetPlayerPawns()
        {
            foreach (var playerController in self.GetPlayerControllers())
            {
                if (playerController.GetPlayerPawn() is not { } pawn)
                {
                    continue;
                }

                yield return pawn;
            }
        }

        public IEnumerable<IBaseEntity> GetTSpawns(bool shouldCheckEnable = true)
        {
            return self.GetEntitiesByClassname("info_player_terrorist", entity =>
            {
                if (!entity.IsValidEntity)
                {
                    return false;
                }

                return !shouldCheckEnable || entity.GetNetVar<bool>("m_bEnabled");
            });
        }

        public IEnumerable<IBaseEntity> GetCtSpawns(bool shouldCheckEnable = true)
        {
            return self.GetEntitiesByClassname("info_player_counterterrorist",
                entity =>
                {
                    if (!entity.IsValidEntity)
                    {
                        return false;
                    }

                    return !shouldCheckEnable || entity.GetNetVar<bool>("m_bEnabled");
                });
        }

        public IEnumerable<IBaseEntity> GetEntitiesByClassname(string classname,
            Func<IBaseEntity, bool>? predicate = null)
        {
            IBaseEntity? entity = null;
            while ((entity = self.FindEntityByClassname(entity, classname)) != null)
            {
                if (predicate is not null && !predicate(entity))
                {
                    continue;
                }

                yield return entity;
            }
        }
    }
}