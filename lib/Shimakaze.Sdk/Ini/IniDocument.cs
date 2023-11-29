namespace Shimakaze.Sdk.Ini;

/// <inheritdoc/>
public sealed class IniDocument : IniDocument<IniSection>
{
    /// <inheritdoc/>
    public IniDocument()
    {
    }

    /// <inheritdoc/>
    public IniDocument(IEnumerable<IniSection> sections) : base(sections)
    {
    }

    /// <inheritdoc/>
    public override IniSection Default { get; } = new() { Name = ";Default;" };
}