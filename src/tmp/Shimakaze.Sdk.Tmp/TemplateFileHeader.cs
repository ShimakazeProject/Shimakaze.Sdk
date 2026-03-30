using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Tmp;

/// <summary>
/// 
/// </summary>
/// <param name="BlockWidth">Width of template in tiles</param>
/// <param name="BlockHeight">Height of template in tiles</param>
/// <param name="BlockImageWidth">Width of tiles (in RA2 always == 60)</param>
/// <param name="BlockImageHeight">Height of tiles (in RA2 always == 30)</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct TemplateFileHeader(
    uint BlockWidth,
    uint BlockHeight,
    uint BlockImageWidth,
    uint BlockImageHeight);
