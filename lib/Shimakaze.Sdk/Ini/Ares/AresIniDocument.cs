namespace Shimakaze.Sdk.Ini.Ares;

/// <inheritdoc/>
public sealed class AresIniDocument : IniDocument<AresIniSection>
{
    /// <inheritdoc/>
    public AresIniDocument()
    {
        Default = new(";Default;", default, new(_defaultSectionKeyComparer));
    }

    /// <inheritdoc/>
    public AresIniDocument(IEnumerable<AresIniSection> sections) : base(sections)
    {
        Default = new(";Default;", default, new(_defaultSectionKeyComparer));
    }

    /// <inheritdoc/>
    public AresIniDocument(IEqualityComparer<string> sectionNameComparer, IEqualityComparer<string>? keyComparer = default) : base(sectionNameComparer, keyComparer)
    {
        Default = new(";Default;", default, new(_defaultSectionKeyComparer));
    }

    /// <inheritdoc/>
    public override AresIniSection Default { get; }
}