using System.Runtime.InteropServices;

using Shimakaze.Sdk.Models.Common;

namespace Shimakaze.Sdk.Models.Tmp;

[StructLayout(LayoutKind.Sequential)]
public struct TileCellHeader
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
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Padding;
    public byte Height;
    public byte LandType;
    public byte SlopeType;
    public RGB TopLeftRadarColor;
    public RGB BottomRightRadarColor;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Padding2;
}