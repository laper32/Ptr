using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Types;

namespace Ptr.Shared.Extensions;

public static class ModSharpExtensions
{
    extension(IModSharp self)
    {
        public void PrintToChatFilter(string message, RecipientFilter filter)
        {
            self.PrintChannelFilter(HudPrintChannel.Chat, message, filter);
        }

        public void PrintToCenterFilter(string message, RecipientFilter filter)
        {
            self.PrintChannelFilter(HudPrintChannel.Center, message, filter);
        }

        public void PrintHintTextFilter(string message, RecipientFilter filter)
        {
            self.PrintChannelFilter(HudPrintChannel.Hint, message, filter);
        }

        public void PrintToConsoleFilter(string message, RecipientFilter filter)
        {
            self.PrintChannelFilter(HudPrintChannel.Console, message, filter);
        }

        public void PrintSayText2Filter(string message, RecipientFilter filter)
        {
            self.PrintChannelFilter(HudPrintChannel.SayText2, message, filter);
        }
    }
}