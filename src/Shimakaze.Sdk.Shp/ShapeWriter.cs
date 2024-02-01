namespace Shimakaze.Sdk.Shp;

/// <summary>
/// SHP 写入器
/// </summary>
public static class ShapeWriter
{
    /// <summary>
    /// 写入SHP到流
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="shp"></param>
    public static void Write(Stream stream, ShapeImage shp)
    {
        stream.Write(shp.Metadata);
        foreach (var frame in shp.Frames)
            stream.Write(frame.Metadata);
        foreach (var frame in shp.Frames)
        {
            if (frame.Indexes is { Length: not 0 })
                stream.Write(frame.Indexes);
        }
    }
}