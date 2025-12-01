using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;

namespace Ptr.Shared.Extensions;

public static class PlayerPawnExtension
{
    extension(IPlayerPawn pawn)
    {
        public void PrintToChat(string message)
        {
            pawn.Print(HudPrintChannel.Chat, message);
        }

        public void PrintToCenter(string message)
        {
            pawn.Print(HudPrintChannel.Center, message);
        }

        public void PrintHintText(string message)
        {
            pawn.Print(HudPrintChannel.Hint, message);
        }

        public void PrintToConsole(string message)
        {
            pawn.Print(HudPrintChannel.Console, message);
        }

        public void PrintSayText2(string message)
        {
            pawn.Print(HudPrintChannel.SayText2, message);
        }
    }
}