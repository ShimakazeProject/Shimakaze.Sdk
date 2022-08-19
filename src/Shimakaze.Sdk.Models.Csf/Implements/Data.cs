namespace Shimakaze.Sdk.Models.Csf.Implements;

internal sealed record class Csf : ICsf
{
    public ICsfMetadata Metadata { get; set; } = ICsfMetadata.Create();
    public IList<ICsfData> Data { get; set; } = new List<ICsfData>();
}

internal sealed record class CsfMetadata : ICsfMetadata
{
    public int Identifier { get; set; }
    public int Version { get; set; }
    public int LabelCount { get; set; }
    public int StringCount { get; set; }
    public int Unknown { get; set; }
    public int Language { get; set; }
}

internal sealed record class CsfData : ICsfData
{
    public int Identifier { get; set; }
    public int StringCount { get; set; }
    public int LabelNameLength { get; set; }
    public string LabelName { get; set; } = string.Empty;
    public IList<ICsfValue> Values { get; set; } = new List<ICsfValue>();
}

internal record class CsfValue : ICsfValue
{
    public int Identifier { get; set; }
    public int ValueLength { get; set; }
    public string Value { get; set; } = string.Empty;
}

internal sealed record class CsfValueExtra : CsfValue, ICsfValueExtra
{
    public int ExtraValueLength { get; set; }
    public string ExtraValue { get; set; } = string.Empty;
}
