namespace Shimakaze.Sdk.Models.Csf;

public interface ICsfData
{
    int Identifier { get; set; }
    int StringCount { get; set; }
    int LabelNameLength { get; set; }
    string LabelName { get; set; }
    IList<ICsfValue> Values { get; set; }
    static ICsfData Create() => new CsfData();
    static ICsfData Create(string labelName!!) => new CsfData
    {
        Identifier = Constants.LBL_FLAG_RAW,
        StringCount = 1,
        LabelNameLength = labelName.Length,
        LabelName = labelName,
    };

    static ICsfData Create(int identifier, int stringCount, int labelNameLength, string labelName, IList<ICsfValue> values)
        => new CsfData
        {
            Identifier = identifier,
            StringCount = stringCount,
            LabelNameLength = labelNameLength,
            LabelName = labelName,
            Values = values
        };

    static ICsfData ReadFrom(BinaryReader reader)
    {
        int flag = Asserts.CheckDataFlags(reader.ReadInt32());
        int count = reader.ReadInt32();
        int lbllength = reader.ReadInt32();
        string lableName = Encoding.ASCII.GetString(reader.ReadBytes(lbllength));

        List<ICsfValue> list = new(count);
        for (int i = 0; i < count; i++)
            list.Add(ICsfValue.ReadFrom(reader));

        return Create(flag, count, lbllength, lableName, list);
    }

    void ReCount()
    {
        StringCount = Values.Count;
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
