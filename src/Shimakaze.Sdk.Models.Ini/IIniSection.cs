using System.Diagnostics.CodeAnalysis;

using Shimakaze.Sdk.Models.Ini.implements;

namespace Shimakaze.Sdk.Models.Ini;
public interface IIniSection : IEnumerable<IIniLine>
{
    IIniLine this[int index] { get; set; }
    string? this[string key] { get; set; }

    IIniLine[]? BeforeSummaries { get; set; }
    string Name { get; set; }
    string? Summary { get; set; }

    void Add(IIniLine kvp);
    bool TryGetValue(string key, [NotNullWhen(true)] out string? result);
}