using Ptr.Shared.Bridge;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;

namespace Ptr.Shared.Extensions;

public static class GameClientExtensions
{
    extension(IGameClient self)
    {
        /// <summary>
        ///     Prints a message to the client's chat
        /// </summary>
        public void PrintToChat(string message)
        {
            self.GetPlayerController()?.PrintToChat(message);
        }

        /// <summary>
        ///     Prints a message to the client's center screen
        /// </summary>
        public void PrintToCenter(string message)
        {
            self.GetPlayerController()?.PrintToCenter(message);
        }

        /// <summary>
        ///     Prints a hint text message to the client
        /// </summary>
        public void PrintHintText(string message)
        {
            self.GetPlayerController()?.PrintHintText(message);
        }

        /// <summary>
        ///     Prints a message to the client's console
        /// </summary>
        public void PrintToConsole(string message)
        {
            self.ConsolePrint(message);
        }

        /// <summary>
        ///     Prints a SayText2 message to the client
        /// </summary>
        public void PrintSayText2(string message)
        {
            self.GetPlayerController()?.PrintSayText2(message);
        }

        public IPlayerPawn? GetPlayerPawn()
        {
            return self.GetPlayerController()?.GetPlayerPawn();
        }

        public void PrintToCenterHtml(string message, int duration = 5)
        {
            if (InterfaceBridge.Instance.EventManager.CreateEvent("show_survival_respawn_status", true) is not { } e)
            {
                return;
            }

            if (!self.IsValid)
            {
                return;
            }

            if (self.IsFakeClient)
            {
                return;
            }

            e.SetString("loc_token", message);
            e.SetInt("duration", duration);
            e.SetInt("userid", self.UserId);
            e.FireToClient(self);
            e.Dispose();
        }
    }
}