namespace Shimakaze.Sdk.Ini.Ares;

/// <inheritdoc/>
public sealed class AresIniDocument : IniDocument<AresIniSection>
{
    /// <inheritdoc />
    public AresIniDocument(
        IEnumerable<AresIniSection> sections,
        IEqualityComparer<string>? sectionNameComparer = default,
        IEqualityComparer<string>? defaultSectionKeyComparer = default)
        : base(sections, sectionNameComparer)
    {
        DefaultSection = new(";Default;", default, new(defaultSectionKeyComparer));
    }

    /// <inheritdoc />
    public AresIniDocument(
        IEqualityComparer<string>? sectionNameComparer = default,
        IEqualityComparer<string>? defaultSectionKeyComparer = default)
        : base(sectionNameComparer)
    {
        DefaultSection = new(";Default;", default, new(defaultSectionKeyComparer));
    }


    /// <inheritdoc/>
    public override AresIniSection DefaultSection { get; }
}