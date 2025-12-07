using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;

namespace Ptr.Shared.Extensions;

public static class PlayerControllerExtension
{
    extension(IPlayerController controller)
    {
        public void PrintToChat(string message)
        {
            controller.Print(HudPrintChannel.Chat, message);
        }

        public void PrintToCenter(string message)
        {
            controller.Print(HudPrintChannel.Center, message);
        }

        public void PrintHintText(string message)
        {
            controller.Print(HudPrintChannel.Hint, message);
        }

        public void PrintToConsole(string message)
        {
            controller.Print(HudPrintChannel.Console, message);
        }

        public void PrintSayText2(string message)
        {
            controller.Print(HudPrintChannel.SayText2, message);
        }

        public void PrintToCenterHtml(string message, int duration = 5)
        {
            controller.GetGameClient()?.PrintToCenterHtml(message, duration);
        }

        public void SetClantag(string clantag)
        {
            var name = controller.PlayerName;

            if (name[^1] == ' ')
            {
                controller.PlayerName = name.TrimEnd(' ');
            }
            else
            {
                controller.PlayerName = name + " ";
            }

            controller.SetClanTag(clantag);
        }
    }
}