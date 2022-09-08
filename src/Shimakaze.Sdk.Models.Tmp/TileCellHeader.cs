using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Models.Tmp;

[StructLayout(LayoutKind.Sequential)]
public record struct TileCellHeader
{
    /// <summary>
    /// tile cell offset
    /// </summary>
    public int X;
    public int Y;
    public uint ExtraDataOffset;
    public uint ZDataOffset;
    public uint ExtraZDataoffset;
    /// <summary>
    /// extra image offset
    /// </summary>
    public int ExtraX;
    public int ExtraY;
    public uint ExtraWidth;
    public uint ExtraHeight;
    public byte Bitfield;

    public byte Padding1;
    public byte Padding2;
    public byte Padding3;

    public byte Height;
    public byte LandType;
    public byte SlopeType;
    public Color TopLeftRadarColor;
    public Color BottomRightRadarColor;

    public byte Padding4;
    public byte Padding5;
    public byte Padding6;
}