namespace Shimakaze.Sdk.Models.Csf;

public interface IReadOnlyCsf
{
    IReadOnlyCsfMetadata Metadata { get; }
    IReadOnlyList<IReadOnlyCsfData> Data { get; }

    static IReadOnlyCsf Create(IReadOnlyCsfMetadata metadata, IReadOnlyList<IReadOnlyCsfData> data)
        => new ReadOnlyCsf(metadata, data);

    public static IReadOnlyCsf Deserialize(BinaryReader reader)
    {
        IReadOnlyCsfMetadata metadata = IReadOnlyCsfMetadata.ReadFrom(reader);
        List<IReadOnlyCsfData> data = new(metadata.LabelCount);
        for (int i = 0; i < metadata.LabelCount; i++)
            data.Add(IReadOnlyCsfData.ReadFrom(reader));

        return Create(metadata, data);
    }

    void WriteTo(BinaryWriter writer)
    {
        Metadata.WriteTo(writer);
        Data.Each(i => i.WriteTo(writer));
    }

    T As<T>() => typeof(T) switch
    {
#pragma warning disable IDE0002
        IReadOnlyCsf => (T)IReadOnlyCsf.Create(
            Metadata.As<IReadOnlyCsfMetadata>(),
            Data.Select(i => i.As<IReadOnlyCsfData>()).ToList().AsReadOnly()),
        ICsf => (T)ICsf.Create(
            Metadata.As<ICsfMetadata>(),
            Data.Select(i => i.As<ICsfData>()).ToList()),
        _ => throw new NotSupportedException()
#pragma warning restore IDE0002
    };


    IReadOnlyCsf ReCount()
    {
        IReadOnlyCsfMetadata head = IReadOnlyCsfMetadata.Create(
            Metadata.Identifier,
            Metadata.Version,
            Data.Count,
            Data.Select(x => x.StringCount).Sum(),
            Metadata.Unknown,
            Metadata.Language);

        return Create(head, Data);
    }
}
