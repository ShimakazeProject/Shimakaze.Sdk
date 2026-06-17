using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Common;

internal readonly record struct BGRA32(byte B, byte G, byte R, byte A)
{
    internal static readonly BGRA32 Transparent = new(0, 0, 0, 0);

    public BGRA32(byte b, byte g, byte r) : this(b, g, r, byte.MaxValue)
    {
    }

    public BGRA32(uint value) : this((byte)((value & 0xFF000000) >> 24), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF))
    {
    }

    public static implicit operator BGRA32(DisplayColor color) => new(color.Blue, color.Green, color.Red);

    public bool Equals(BGRA32 bGRA)
    {
        return B == bGRA.B &&
               G == bGRA.G &&
               R == bGRA.R &&
               A == bGRA.A;
    }

    public override int GetHashCode()
    {
        int hashCode = 931614316;
        hashCode = (hashCode * -1521134295) + B.GetHashCode();
        hashCode = (hashCode * -1521134295) + G.GetHashCode();
        hashCode = (hashCode * -1521134295) + R.GetHashCode();
        hashCode = (hashCode * -1521134295) + A.GetHashCode();
        return hashCode;
    }
}
