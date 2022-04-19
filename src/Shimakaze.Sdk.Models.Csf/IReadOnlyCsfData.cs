namespace Shimakaze.Sdk.Models.Csf;

public interface IReadOnlyCsfData
{
    int Identifier { get; }
    int StringCount { get; }
    int LabelNameLength { get; }
    string LabelName { get; }
    IReadOnlyList<IReadOnlyCsfValue> Values { get; }

    static IReadOnlyCsfData Create(int identifier, int stringCount, int labelNameLength, string labelName, IReadOnlyList<IReadOnlyCsfValue> values)
        => new ReadOnlyCsfData(identifier, stringCount, labelNameLength, labelName, values);

    static IReadOnlyCsfData ReadFrom(BinaryReader reader)
    {
        int flag = Asserts.CheckDataFlags(reader.ReadInt32());
        int count = reader.ReadInt32();
        int lbllength = reader.ReadInt32();
        string lableName = Encoding.ASCII.GetString(reader.ReadBytes(lbllength));

        List<IReadOnlyCsfValue> list = new(count);
        for (int i = 0; i < count; i++)
            list.Add(IReadOnlyCsfValue.ReadFrom(reader));

        return Create(flag, count, lbllength, lableName, list.AsReadOnly());
    }
    void WriteTo(BinaryWriter writer)
    {
        writer.Write(Identifier);
        writer.Write(StringCount);
        writer.Write(LabelNameLength);
        writer.Write(Encoding.ASCII.GetBytes(LabelName));
        Values.Each(i => i.WriteTo(writer));
    }

    T As<T>() => typeof(T) switch
    {
#pragma warning disable IDE0002
        IReadOnlyCsfData => (T)IReadOnlyCsfData.Create(
                Identifier,
                StringCount,
                LabelNameLength,
                LabelName,
                Values.Select(i => i.As<IReadOnlyCsfValue>()).ToList().AsReadOnly()),
        ICsfData => (T)ICsfData.Create(
                Identifier,
                StringCount,
                LabelNameLength,
                LabelName,
                Values.Select(i => i.As<ICsfValue>()).ToList()),
        _ => throw new NotSupportedException()
#pragma warning restore IDE0002
    };
}
