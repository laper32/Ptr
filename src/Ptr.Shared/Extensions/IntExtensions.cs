using System.Runtime.CompilerServices;

namespace Ptr.Shared.Extensions;

public static class IntExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToIpV4(this int self)
    {
        return
            $"{((self & 0xFF000000) >> 24) & 0xFF}.{((self & 0x00FF0000) >> 16) & 0xFF}.{((self & 0x0000FF00) >> 8) & 0xFF}.{((self & 0x000000FF) >> 0) & 0xFF}";
    }
}