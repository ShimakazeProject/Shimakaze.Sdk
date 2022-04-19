namespace Shimakaze.Sdk.Models.Csf;

public interface ICsf
{
    ICsfMetadata Metadata { get; set; }
    IList<ICsfData> Data { get; set; }
    static ICsf Create() => new Impliments.Csf();

    static ICsf Create(ICsfMetadata metadata, IList<ICsfData> data) => new Impliments.Csf
    {
        Metadata = metadata,
        Data = data
    };

    public static ICsf Deserialize(BinaryReader reader)
    {
        ICsfMetadata metadata = ICsfMetadata.ReadFrom(reader);
        List<ICsfData> data = new(metadata.LabelCount);
        for (int i = 0; i < metadata.LabelCount; i++)
            data.Add(ICsfData.ReadFrom(reader));

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
    ICsf ReCount()
    {
        Data.Each(i => i.ReCount());
        ICsfMetadata head = Metadata;
        head.LabelCount = Data.Count;
        head.StringCount = Data.Select(x => x.StringCount).Sum();
        return this;
    }
}
