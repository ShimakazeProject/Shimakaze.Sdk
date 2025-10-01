namespace Shimakaze.Sdk.Hva;

/// <summary>
/// </summary>
public sealed record class HvaFrame(Memory<HvaMatrix> Matrices)
{
    /// <summary>
    /// Transformation matrix for each section
    /// </summary>
    public Memory<HvaMatrix> Matrices { get; set; } = Matrices;
}
