using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Map;

/// <summary>
/// 地块信息
/// </summary>
/// <remarks>
/// <see href="https://modenc.renegadeprojects.com/IsoMapPack5"/>
/// </remarks>
/// <param name="X">地图单元坐标</param>
/// <param name="Y">地图单元坐标</param>
/// <param name="TileIndex">单元格瓦片索引</param>
/// <param name="TileSubIndex">子索引</param>
/// <param name="Level">高度</param>
/// <param name="IceGrowth">冰生长</param>
[StructLayout(LayoutKind.Sequential, Size = 11)]
public readonly record struct IsoMapPack5Tile(short X, short Y, int TileIndex, byte TileSubIndex, byte Level, byte IceGrowth);
