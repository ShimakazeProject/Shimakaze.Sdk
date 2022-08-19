using Shimakaze.Sdk.Models.Ini.implements;

namespace Shimakaze.Sdk.Models.Ini;

public interface IIniLine
{
    bool IsEmptyKey { get; }
    bool IsEmptyValue { get; }
    bool IsEmptySummary { get; }
    string? Key { get; set; }
    string? Summary { get; set; }
    IniValue? Value { get; set; }

    string ToString(bool ignoreSummary = false);
}