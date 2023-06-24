namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbBody
/// </summary>
public record struct LimbBody
{
    /// <summary>
    /// SpanStart
    /// </summary>
    public int[] SpanStart { get; set; }
    /// <summary>
    /// SpanEnd
    /// </summary>
    public int[] SpanEnd { get; set; }
    /// <summary>
    /// Data
    /// </summary>
    public SpanStruct[] Data { get; set; }
}
