using System.Diagnostics;
using System.Runtime.InteropServices;
using Sharp.Shared.Types;

namespace Ptr.Shared.Types;

[StructLayout(LayoutKind.Sequential)]
public struct QAngle(float pitch, float yaw, float roll)
{
    /// <summary>
    ///     Pitch (Up / Down)
    /// </summary>
    public float Pitch { get; set; } = pitch;

    /// <summary>
    ///     Yaw (Left / Right)
    /// </summary>
    public float Yaw { get; set; } = yaw;

    /// <summary>
    ///     Roll (Fall over)
    /// </summary>
    public float Roll { get; set; } = roll;

    public QAngle() : this(0.0f, 0.0f, 0.0f)
    {
    }


    public QAngle(QAngle other) : this(other.Pitch, other.Yaw, other.Roll)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is QAngle h)
        {
            return this == h;
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + Pitch.GetHashCode();
            hash = hash * 23 + Yaw.GetHashCode();
            hash = hash * 23 + Roll.GetHashCode();

            return hash;
        }
    }

    public static bool operator ==(QAngle a, QAngle b)
    {
        return MathF.Abs(a.Pitch - b.Pitch) < 0.01f && MathF.Abs(a.Yaw - b.Yaw) < 0.01f &&
               MathF.Abs(a.Roll - b.Roll) < 0.01f;
    }

    public static bool operator !=(QAngle a, QAngle b)
    {
        return !(a == b);
    }


    public float this[int key]
    {
        readonly get
            => key switch
            {
                0 => Pitch,
                1 => Yaw,
                2 => Roll,
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        set
        {
            switch (key)
            {
                case 0:
                    Pitch = value;

                    break;
                case 1:
                    Yaw = value;

                    break;
                case 2:
                    Roll = value;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }

    public static QAngle operator +(QAngle a, QAngle b)
    {
        return new QAngle(a.Pitch + b.Pitch, a.Yaw + b.Yaw, a.Roll + b.Roll);
    }

    public static QAngle operator -(QAngle a, QAngle b)
    {
        return new QAngle(a.Pitch - b.Pitch, a.Yaw - b.Yaw, a.Roll - b.Roll);
    }

    public static QAngle operator *(QAngle vec, float fl)
    {
        return new QAngle(vec.Pitch * fl, vec.Yaw * fl, vec.Roll * fl);
    }

    public static QAngle operator *(QAngle a, QAngle b)
    {
        return new QAngle(a.Pitch * b.Pitch, a.Yaw * b.Yaw, a.Roll * b.Roll);
    }

    public static QAngle operator +(QAngle vec, float fl)
    {
        return new QAngle(vec.Pitch + fl, vec.Yaw + fl, vec.Roll + fl);
    }

    public static QAngle operator -(QAngle vec, float fl)
    {
        return new QAngle(vec.Pitch - fl, vec.Yaw - fl, vec.Roll - fl);
    }

    public static QAngle operator /(QAngle vec, float fl)
    {
        Debug.Assert(fl != 0.0f);
        var vf = 1.0f / fl;

        return new QAngle(vec.Pitch * vf, vec.Yaw * vf, vec.Roll * vf);
    }

    public static QAngle operator /(QAngle a, QAngle b)
    {
        Debug.Assert(b.Pitch != 0.0f && b.Yaw != 0.0f && b.Roll != 0.0f);

        return new QAngle(a.Pitch / b.Pitch, a.Yaw / b.Yaw, a.Roll / b.Roll);
    }

    public float Length()
    {
        return MathF.Sqrt(LengthSqr());
    }

    public float LengthSqr()
    {
        return Pitch * Pitch + Yaw * Yaw + Roll * Roll;
    }

    /// <summary>
    ///     Normalize
    /// </summary>
    public void Normalize()
    {
        var length = MathF.Sqrt(Pitch * Pitch + Yaw * Yaw + Roll * Roll);
        Pitch /= length;
        Yaw /= length;
        Roll /= length;
    }

    public static implicit operator QAngle(Vector x)
    {
        return new QAngle(x.X, x.Y, x.Z);
    }

    public static implicit operator Vector(QAngle x)
    {
        return new Vector(x.Pitch, x.Yaw, x.Roll);
    }

    public string ToEkvString()
    {
        return $"{Pitch:F6} {Yaw:F6} {Roll:F6}";
    }

    public override string ToString()
    {
        return $"{{{Pitch:F6}, {Yaw:F6}, {Roll:F6}}}";
    }
}