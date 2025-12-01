using Sharp.Shared.Types;
using Sharp.Shared.Units;

namespace Ptr.Shared.Extensions;

public static class StringCommandExtension
{
    extension(StringCommand command)
    {
        public SteamID? ParseSteamId(int index)
        {
            try
            {
                SteamID steamId = Convert.ToUInt64(command.GetArg(index));
                if (!steamId.IsValidUserId())
                {
                    return null;
                }

                return steamId;
            }
            catch
            {
                return null;
            }
        }

        public void ParseVector(int startIndex, out Vector position)
        {
            position = new Vector();
            //eg: start index = 2, means that 2, 3, 4 are x, y, z, respectively.

            var x = command.TryGet<float?>(startIndex) ?? 0;
            var y = command.TryGet<float?>(startIndex + 1) ?? 0;
            var z = command.TryGet<float?>(startIndex + 2) ?? 0;

            position = new Vector(x, y, z);
        }
    }
}