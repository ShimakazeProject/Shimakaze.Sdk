using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Models.Ini.Map;

[StructLayout(LayoutKind.Explicit, Size = 11)]
struct IsoMapPack5Tile
{
    [FieldOffset(0)] public short X;
    [FieldOffset(2)] public short Y;
    [FieldOffset(4)] public int TileIndex;
    [FieldOffset(8)] public byte TileSubIndex;
    [FieldOffset(9)] public byte Level;
    [FieldOffset(10)] public byte IceGrowth;
}
