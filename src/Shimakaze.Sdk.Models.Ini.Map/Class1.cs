using Shimakaze.Sdk.Models.Ini.implements;


namespace Shimakaze.Sdk.Models.Ini.Map;
public class Class1
{
}


public class MapDocument : IniDocument
{
    public IIniSection Basic => this[nameof(Basic)];
}