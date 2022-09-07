using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Models.Shp;

[StructLayout(LayoutKind.Sequential)]
public struct FrameHeader
{
    public ushort X;
    public ushort Y;
    public ushort Width;
    public ushort Height;
    public FrameFlag Flags;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Align;
    public uint Color;
    public uint Reserved;
    public uint Offset;
}
