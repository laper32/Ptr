using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Types;

namespace Ptr.Shared.Extensions;

public static class ModSharpExtensions
{
    public static void PrintToChatFilter(this IModSharp self, string message, RecipientFilter filter)
    {
        self.PrintChannelFilter(HudPrintChannel.Chat, message, filter);
    }

    public static void PrintToCenterFilter(this IModSharp self, string message, RecipientFilter filter)
    {
        self.PrintChannelFilter(HudPrintChannel.Center, message, filter);
    }

    public static void PrintHintTextFilter(this IModSharp self, string message, RecipientFilter filter)
    {
        self.PrintChannelFilter(HudPrintChannel.Hint, message, filter);
    }

    public static void PrintToConsoleFilter(this IModSharp self, string message, RecipientFilter filter)
    {
        self.PrintChannelFilter(HudPrintChannel.Console, message, filter);
    }

    public static void PrintSayText2Filter(this IModSharp self, string message, RecipientFilter filter)
    {
        self.PrintChannelFilter(HudPrintChannel.SayText2, message, filter);
    }
}