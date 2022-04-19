namespace Shimakaze.Sdk.Models.Csf;

public interface ICsfValueExtra : ICsfValue
{
    int ExtraValueLength { get; set; }
    string ExtraValue { get; set; }
    static new ICsfValueExtra Create() => new CsfValueExtra();
}