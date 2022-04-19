namespace Shimakaze.Sdk.Models.Csf;

public interface ICsfMetadata
{
    int Identifier { get; set; }
    int Version { get; set; }
    int LabelCount { get; set; }
    int StringCount { get; set; }
    int Unknown { get; set; }
    int Language { get; set; }
    static ICsfMetadata Create() => new CsfMetadata();
    static ICsfMetadata Create(int identifier, int version, int labelCount, int stringCount, int unknown, int language) => new CsfMetadata
    {
        Identifier = identifier,
        Version = version,
        LabelCount = labelCount,
        StringCount = stringCount,
        Unknown = unknown,
        Language = language
    };

    static ICsfMetadata ReadFrom(BinaryReader reader) => Create(
             Asserts.CheckMetadataFlags(reader.ReadInt32()),
             reader.ReadInt32(),
             reader.ReadInt32(),
             reader.ReadInt32(),
             reader.ReadInt32(),
             reader.ReadInt32());

    void WriteTo(BinaryWriter writer)
    {
        writer.Write(Identifier);
        writer.Write(Version);
        writer.Write(LabelCount);
        writer.Write(StringCount);
        writer.Write(Unknown);
        writer.Write(Language);
    }

    T As<T>() => typeof(T) switch
    {
#pragma warning disable IDE0002
        IReadOnlyCsfMetadata => (T)IReadOnlyCsfMetadata.Create(
            Identifier,
            Version,
            LabelCount,
            StringCount,
            Unknown,
            Language),
        ICsfMetadata => (T)ICsfMetadata.Create(
            Identifier,
            Version,
            LabelCount,
            StringCount,
            Unknown,
            Language),
        _ => throw new NotSupportedException()
#pragma warning restore IDE0002
    };
}
