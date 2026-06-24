namespace Shimakaze.Sdk.Engine.Shp;

/// <summary>
/// Represents the source image files for a single SHP frame.
/// <br />
/// Used by <see cref="ShapeMaker"/> to build a complete frame from object,
/// shadow, and house-colour mask images.
/// </summary>
/// <param name="Object">The main object image file.</param>
/// <param name="Shadow">The shadow image file, or <see langword="null"/> if no shadow.</param>
/// <param name="House">The house-colour mask image file, or <see langword="null"/> if no house colour.</param>
#pragma warning disable CA1720 // 标识符包含类型名称
public sealed record class ShapeFrameSource(FileInfo Object, FileInfo? Shadow, FileInfo? House);
#pragma warning restore CA1720 // 标识符包含类型名称
