namespace Shimakaze.Sdk.Ini;

/// <inheritdoc/>
public sealed class IniDocument : IniDocument<IniSection>
{
    /// <inheritdoc />
    public IniDocument(
        IEnumerable<IniSection> sections,
        IEqualityComparer<string>? sectionNameComparer = default,
        IEqualityComparer<string>? defaultSectionKeyComparer = default)
        : base(sections, sectionNameComparer)
    {
        DefaultSection = new(";Default;", new(defaultSectionKeyComparer));
    }

    /// <inheritdoc />
    public IniDocument(
        IEqualityComparer<string>? sectionNameComparer = default,
        IEqualityComparer<string>? defaultSectionKeyComparer = default)
        : base(sectionNameComparer)
    {
        DefaultSection = new(";Default;", new(defaultSectionKeyComparer));
    }


    /// <inheritdoc/>
    public override IniSection DefaultSection { get; }
}