using Shimakaze.Sdk.Models.Ini.implements;

namespace Shimakaze.Sdk.Models.Ini;

public interface IIniDocument : IEnumerable<IIniSection>
{
    IIniSection this[string section] { get; set; }
    string? this[string section, string key] { get; set; }

    IIniSection Default { get; }
    IList<IIniSection> Sections { get; }

    void Add(IIniSection section);
    bool Remove(string section);
}