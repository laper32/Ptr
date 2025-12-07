using Sharp.Shared.Types;

namespace Ptr.Shared.Extensions;

public static class VectorExtensions
{
    /// <summary>
    ///     Stolen from @prefix's code. Thanks dude :kekw:
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static string ToEKvString(this Vector self)
    {
        return $"{self.X:F6} {self.Y:F6} {self.Z:F6}";
    }
}