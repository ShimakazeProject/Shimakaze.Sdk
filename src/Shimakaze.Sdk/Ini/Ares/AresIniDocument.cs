namespace Shimakaze.Sdk.Ini.Ares;

/// <inheritdoc/>
public sealed class AresIniDocument : IniDocument<AresIniSection>
{
    /// <inheritdoc/>
    public AresIniDocument()
    {
    }

    /// <inheritdoc/>
    public AresIniDocument(IEnumerable<AresIniSection> sections) : base(sections)
    {
    }

    /// <inheritdoc/>
    public override AresIniSection Default { get; } = new() { Name = ";Default;" };
}
