namespace Shimakaze.Sdk.Models.Csf.Impliments;
internal sealed record class ReadOnlyCsf(IReadOnlyCsfMetadata Metadata, IReadOnlyList<IReadOnlyCsfData> Data) : IReadOnlyCsf;
internal sealed record class ReadOnlyCsfMetadata(int Identifier, int Version, int LabelCount, int StringCount, int Unknown, int Language) : IReadOnlyCsfMetadata;
internal sealed record class ReadOnlyCsfData(int Identifier, int StringCount, int LabelNameLength, string LabelName, IReadOnlyList<IReadOnlyCsfValue> Values) : IReadOnlyCsfData;
internal sealed record class ReadOnlyCsfValue(int Identifier, int ValueLength, string Value) : IReadOnlyCsfValue;
internal sealed record class ReadOnlyCsfValueExtra(int Identifier, int ValueLength, string Value, int ExtraValueLength, string ExtraValue) : IReadOnlyCsfValueExtra;
