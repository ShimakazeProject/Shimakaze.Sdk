namespace Shimakaze.Sdk.Models.Csf;

public interface IReadOnlyCsfValueExtra : IReadOnlyCsfValue
{
    int ExtraValueLength { get; }
    string ExtraValue { get; }
}