using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace Ptr.Shared.Extensions;

public static class ClientManagerExtensions
{
    extension(IClientManager self)
    {
        public IEnumerable<IGameClient> GetBots()
        {
            return self.GetGameClients(true).Where(client => client.IsFakeClient);
        }

        public IEnumerable<IGameClient> GetAlive()
        {
            return self.GetGameClients(true).Where(client => client.GetPlayerPawn() is { IsAlive: true });
        }

        public IEnumerable<IGameClient> GetCTs()
        {
            return self.GetGameClients(true).Where(client => client.GetPlayerPawn() is { Team: CStrikeTeam.CT });
        }

        public IEnumerable<IGameClient> GetTs()
        {
            return self.GetGameClients(true).Where(client => client.GetPlayerPawn() is { Team: CStrikeTeam.TE });
        }

        public IEnumerable<IGameClient> GetSpecs()
        {
            return self.GetGameClients(true).Where(client => client.GetPlayerPawn() is { Team: CStrikeTeam.Spectator });
        }

        public IEnumerable<IGameClient> GetTeamClients(CStrikeTeam team)
        {
            return self.GetGameClients(true).Where(client => client.GetPlayerPawn()?.Team == team);
        }

        public IEnumerable<IGameClient> GetAliveTs()
        {
            return self.GetGameClients(true).Where(x => x.GetPlayerPawn() is { Team: CStrikeTeam.TE, IsAlive: true });
        }

        public IEnumerable<IGameClient> GetAliveCTs()
        {
            return self.GetGameClients(true).Where(x => x.GetPlayerPawn() is { Team: CStrikeTeam.CT, IsAlive: true });
        }

        public IEnumerable<IGameClient> GetAliveTeamMembers(CStrikeTeam team)
        {
            return self.GetGameClients(true)
                .Where(x => x.GetPlayerPawn() is { IsAlive: true } pawn && pawn.Team == team);
        }

        public bool IsCtAlive()
        {
            return self.GetAliveCTs().Any();
        }

        public bool IsTAlive()
        {
            return self.GetAliveTs().Any();
        }

        public bool IsTeamAlive(CStrikeTeam team)
        {
            return self.GetAliveTeamMembers(team).Any();
        }
    }
}