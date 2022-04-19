namespace Shimakaze.Sdk.Models.Csf;

public interface IReadOnlyCsfMetadata
{
    int Identifier { get; }
    int Version { get; }
    int LabelCount { get; }
    int StringCount { get; }
    int Unknown { get; }
    int Language { get; }

    static IReadOnlyCsfMetadata Create(int identifier, int version, int labelCount, int stringCount, int unknown, int language)
        => new ReadOnlyCsfMetadata(identifier, version, labelCount, stringCount, unknown, language);

    static IReadOnlyCsfMetadata ReadFrom(BinaryReader reader) => Create(
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
