using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Tmp;

/// <summary>
/// 
/// </summary>
/// <param name="X">Position of the tile in the template</param>
/// <param name="Y">Position of the tile in the template</param>
/// <param name="ExtraDataOffset">Offset to extra data (? CHECK)</param>
/// <param name="ZDataoffset">
/// Offset to height data (relative to start of header) <br />
/// always == 952
/// </param>
/// <param name="ExtraZDataoffset">
/// Unknown (from XCC source) <br />
/// always seems to be 0
/// </param>
/// <param name="ExtraX">Position that the extra data should be drawn</param>
/// <param name="ExtraY">Position that the extra data should be drawn</param>
/// <param name="ExtraWidth">Size of extra data</param>
/// <param name="ExtraHeight">Size of extra data</param>
/// <param name="Flags">
/// The 3 least significant bits out of the 8 bits are used as boolean (0/1) flags for three tile cell attributes: HasExtraData (least significant), HasZData, and HasDamagedData respectively. The game checks these bits to process the extra-data or z-data or the damaged logic. If HasExtraData/HasZData bits are set to 0, the game won't use its respective extra-data or z-data even if present. If the HasDamagedData bit is set to 0, the game randomly picks the tile cell from one of the TMP variant files, if present. These variants are identified by a single character suffix (from A to G) at the end of the filename, e.g. clear01.tem, clear01a.tem, clear01b.tem etc. HasDamagedData bit is set to 1 for bridges, so that the bridge tiles are not randomized. The remaining leading 5 bits out of the 8 bits are not used irrespective of the values present in those. For example, a byte value of 0xCB or 0x03 or 0x83 or 0x4B, all have the same result of binary xxxxx011 which means HasDamagedData bit is not set, HasZData bit is set and HasExtraData bit is set.
/// </param>
/// <param name="Height">Height of tile</param>
/// <param name="LandType">Numbering used for land characteristics.</param>
/// <param name="SlopeType">Type of ramp (MEANING???)</param>
/// <param name="TopLeftRadarColor">
/// The top-left radar color and bottom-right radar color being different, enhances the demarcation in the minimap where the adjacent tiles/terrain change in the map. These also give a sense of the light source being at top-left in the minimap. Compare to the unused LowRadarColor and HighRadarColor.
/// </param>
/// <param name="BottomRightRadarColor"><paramref name="TopLeftRadarColor"/></param>
/// <param name="Padding1"></param>
/// <param name="Padding2"></param>
/// <param name="Padding3"></param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct TemplateTileCellHeader(
    int X,
    int Y,
    uint ExtraDataOffset,
    uint ZDataoffset,
    uint ExtraZDataoffset,
    int ExtraX,
    int ExtraY,
    uint ExtraWidth,
    uint ExtraHeight,
    TemplateTileCellFlags Flags,
    byte Height,
    byte LandType,
    byte SlopeType,
    RGBColor TopLeftRadarColor,
    RGBColor BottomRightRadarColor,
    byte Padding1,
    byte Padding2,
    byte Padding3);
