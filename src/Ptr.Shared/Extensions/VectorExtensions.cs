using Sharp.Shared.Types;

namespace Ptr.Shared.Extensions;

public static class VectorExtensions
{
    public static string ToEKvString(this Vector self)
    {
        return $"{self.X:F6} {self.Y:F6} {self.Z:F6}";
    }
}