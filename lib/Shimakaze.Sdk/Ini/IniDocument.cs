namespace Shimakaze.Sdk.Ini;

/// <inheritdoc/>
public sealed class IniDocument : IniDocument<IniSection>
{
    /// <inheritdoc/>
    public IniDocument()
    {
        Default = new(";Default;", new(_defaultSectionKeyComparer));
    }

    /// <inheritdoc/>
    public IniDocument(IEnumerable<IniSection> sections) : base(sections)
    {
        Default = new(";Default;", new(_defaultSectionKeyComparer));
    }

    /// <inheritdoc/>
    public IniDocument(IEqualityComparer<string> sectionNameComparer, IEqualityComparer<string>? keyComparer = default) : base(sectionNameComparer, keyComparer)
    {
        Default = new(";Default;", new(_defaultSectionKeyComparer));
    }

    /// <inheritdoc/>
    public override IniSection Default { get; }
}