namespace Shimakaze.Sdk.Hva;

/// <summary>
/// 
/// </summary>
public record struct HvaFrame
{
    /// <summary>
    /// Transformation matrix for each section
    /// </summary>
    public HvaMatrix[] Matrices;
}