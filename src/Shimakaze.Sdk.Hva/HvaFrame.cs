namespace Shimakaze.Sdk.Hva;

/// <summary>
/// </summary>
public record class HvaFrame
{
    /// <summary>
    /// Transformation matrix for each section
    /// </summary>
    public HvaMatrix[] Matrices { get; set; } = [];
}