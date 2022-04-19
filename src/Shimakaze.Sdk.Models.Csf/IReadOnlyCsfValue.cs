namespace Shimakaze.Sdk.Models.Csf;

public interface IReadOnlyCsfValue
{
    int Identifier { get; }
    int ValueLength { get; }
    string Value { get; }
    static IReadOnlyCsfValue Create(int identifier, int valueLength, string value)
        => new ReadOnlyCsfValue(identifier, valueLength, value);
    static IReadOnlyCsfValueExtra Create(int identifier, int valueLength, string value, int extraValueLength, string extraValue)
        => new ReadOnlyCsfValueExtra(identifier, valueLength, value, extraValueLength, extraValue);

    static IReadOnlyCsfValue ReadFrom(BinaryReader reader)
    {
        int flag = Asserts.CheckValueFlags(reader.ReadInt32());
        int length = reader.ReadInt32() << 1;
        byte[]? data = CodingValue(reader.ReadBytes(length));
        string value = Encoding.Unicode.GetString(data);

        if (Asserts.TryValueFlagsIsExtra(flag))
        {
            int elength = reader.ReadInt32();
            string extra = Encoding.ASCII.GetString(reader.ReadBytes(length));
            return Create(flag, length, value, elength, extra);
        }
        return Create(flag, length, value);
    }

    void WriteTo(BinaryWriter writer)
    {
        writer.Write(Identifier);
        writer.Write(ValueLength);
        writer.Write(CodingValue(Encoding.Unicode.GetBytes(Value)));
        if (this is IReadOnlyCsfValueExtra extra)
        {
            writer.Write(extra.ExtraValueLength);
            writer.Write(Encoding.ASCII.GetBytes(extra.ExtraValue));
        }
    }

    T As<T>()
    {
#pragma warning disable IDE0002
        IReadOnlyCsfValueExtra? extra = this as IReadOnlyCsfValueExtra;
        return typeof(T) switch
        {
            ICsfValueExtra => (T)ICsfValueExtra.Create(
                Identifier,
                ValueLength,
                Value,
                extra?.ExtraValueLength ?? 0,
                extra?.ExtraValue ?? string.Empty),
            ICsfValue => (T)ICsfValue.Create(
                Identifier,
                ValueLength,
                Value),
            IReadOnlyCsfValueExtra => (T)IReadOnlyCsfValueExtra.Create(
                Identifier,
                ValueLength,
                Value,
                extra?.ExtraValueLength ?? 0,
                extra?.ExtraValue ?? string.Empty),
            IReadOnlyCsfValue => (T)IReadOnlyCsfValue.Create(
                Identifier,
                ValueLength,
                Value),
            _ => throw new NotSupportedException()
        };
#pragma warning restore IDE0002
    }
    /// <summary>
    /// 值字符串 编/解码<br/>
    /// CSF文档中的Unicode编码内容都是按位异或的<br/>
    /// 这个方法使用for循环实现
    /// </summary>
    /// <param name="valueData">内容</param>
    /// <param name="valueDataLength">内容长度</param>
    static byte[] CodingValue(byte[] valueData, int start = 0, int? valueDataLength = null)
        => ICsfValue.CodingValue(valueData, start, valueDataLength);
}
