using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Shimakaze.Sdk.Data.Serialization;

namespace Shimakaze.Sdk.Data.Csf.Serialization;

/// <summary>
/// CsfSerializer.
/// </summary>
public sealed class CsfSerializer : IBinarySerializer<CsfDocument, CsfSerializerOptions>
{
    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="reader">reader.</param>
    /// <param name="options">options.</param>
    /// <returns>CsfDocument.</returns>
    public static CsfDocument Deserialize(BinaryReader reader, CsfSerializerOptions? options = null)
    {
        options ??= CsfSerializerOptions.Default;
        const int sizeofLabel = sizeof(int) * 3;
        const int sizeofValue = sizeof(int) * 2;
        int i, j, flag, count, length;
        string label, value;
        CsfData data;
        CsfDocument csf;
        unsafe
        {
            fixed (byte* ptr = reader.ReadBytes(sizeof(CsfMetadata)))
            {
                csf = new(Marshal.PtrToStructure<CsfMetadata>((nint)ptr));
                Debug.WriteLine(csf.Metadata);
                CsfThrowHelper.IsCsfFile(*(int*)ptr);
            }

            for (i = 0; i < csf.Metadata.LabelCount; i++)
            {
                fixed (byte* ptr = reader.ReadBytes(sizeofLabel))
                {
                    int* p = (int*)ptr;
                    flag = *p++;
                    count = *p++;
                    length = *p++;
                    Debug.WriteLine($"#{i:D6}", "label");
                    Debug.WriteLine($"  Offset: 0x{reader.BaseStream.Position - sizeofLabel:X8}", "label");
                    Debug.WriteLine($"  Flag: 0x{flag:X8}", "label");
                    Debug.WriteLine($"  String Count: {count}", "label");
                    Debug.WriteLine($"  Label Name Length: {length}", "label");
                    CsfThrowHelper.IsLabel(flag, () => new object[] { i, reader.BaseStream.Position - sizeofLabel });
                }

                fixed (byte* ptr = reader.ReadBytes(length))
                {
                    label = Encoding.ASCII.GetString(ptr, length);
                    Debug.WriteLine($"  Label Name: {label}", "label");
                }

                data = new(flag, count, length, label);
                csf.Add(data);

                for (j = 0; j < count; j++)
                {
                    fixed (byte* ptr = reader.ReadBytes(sizeofValue))
                    {
                        int* p = (int*)ptr;
                        flag = *p++;
                        length = *p++;
                        Debug.WriteLine($"  Values Info:", "value");
                        Debug.WriteLine($"    Offset: 0x{reader.BaseStream.Position - sizeofValue:X8}", "value");
                        Debug.WriteLine($"    Flag: 0x{flag:X8}", "value");
                        Debug.WriteLine($"    String Length: {length}", "value");
                        CsfThrowHelper.IsStringOrExtraString(flag, () => new object[] { i, j, reader.BaseStream.Position - sizeofValue });
                    }

                    if (length <= 0)
                    {
                        Debug.WriteLine($"    This value haven't any characters!", "value");
                        continue;
                    }

                    fixed (byte* ptr = reader.ReadBytes(length << 1))
                    {
                        CodingValue(ptr, length << 1);
                        value = Encoding.Unicode.GetString(ptr, length << 1);
                        Debug.WriteLine($"    Value: {value}", "value");
                    }

                    if (flag is not CsfConstants.StrwFlgRaw)
                    {
                        data.Add(new CsfValue(flag, length, value));
                        continue;
                    }

                    int extralength = reader.ReadInt32();
                    Debug.WriteLine($"    Extra:", "extra");
                    Debug.WriteLine($"      Extra Length: {extralength}", "extra");
                    string extra = Encoding.ASCII.GetString(reader.ReadBytes(extralength));
                    Debug.WriteLine($"      Extra: {extra}", "extra");
                    data.Add(new CsfValueExtra(flag, length, value, extralength, extra));
                }
            }
        }

        return csf;
    }

    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="options">options.</param>
    /// <returns>CsfDocument.</returns>
    public static CsfDocument Deserialize(Stream stream, CsfSerializerOptions? options = null)
    {
        using BinaryReader br = new(stream);
        return Deserialize(br, options);
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="writer">writer.</param>
    /// <param name="document">document.</param>
    /// <param name="options">options.</param>
    public static void Serialize(BinaryWriter writer, CsfDocument document, CsfSerializerOptions? options = null)
    {
        options ??= CsfSerializerOptions.Default;
        unsafe
        {
            nint ptr = Marshal.AllocHGlobal(sizeof(CsfMetadata));
            try
            {
                Marshal.StructureToPtr(document.Metadata, ptr, false);
                writer.Write(new Span<byte>((byte*)ptr, sizeof(CsfMetadata)));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        foreach (CsfData item in document.Data)
        {
            writer.Write(item.Identifier);
            writer.Write(item.StringCount);
            writer.Write(item.LabelNameLength);
            writer.Write(Encoding.ASCII.GetBytes(item.LabelName));
            if (!item.Values.Any())
            {
                writer.Write(CsfConstants.StrFlagRaw);
                writer.Write(0);
                continue;
            }

            foreach (CsfValue value in item.Values)
            {
                writer.Write(value.Identifier);
                writer.Write(value.ValueLength);
                writer.Write(CodingValue(Encoding.Unicode.GetBytes(value.Value)));
                if (value is CsfValueExtra extra)
                {
                    writer.Write(extra.ExtraValueLength);
                    writer.Write(Encoding.ASCII.GetBytes(extra.ExtraValue));
                }
            }
        }
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="document">document.</param>
    /// <param name="options">options.</param>
    public static void Serialize(Stream stream, CsfDocument document, CsfSerializerOptions? options = null)
    {
        using BinaryWriter bw = new(stream, Encoding.Default, true);
        Serialize(bw, document, options);
    }

    /// <summary>
    /// Value string Encode/decode<br/>
    /// The Unicode encoded content in CSF documents are all bitwise isochronous<br/>
    /// This method is implemented using a for loop.
    /// </summary>
    /// <param name="data">data.</param>
    /// <param name="length">length.</param>
    /// <param name="start">start index.</param>
    public static unsafe void CodingValue(byte* data, int length, int start = 0)
    {
        for (int i = 0; i < length; i++)
        {
            data[start + i] = (byte)~data[start + i];
        }
    }

    /// <summary>
    /// Value string Encode/decode<br/>
    /// The Unicode encoded content in CSF documents are all bitwise isochronous<br/>
    /// This method is implemented using a for loop.
    /// </summary>
    /// <param name="data">data.</param>
    /// <returns>result.</returns>
    public static byte[] CodingValue(byte[] data)
    {
        unsafe
        {
            fixed (byte* ptr = data)
            {
                CodingValue(ptr, data.Length);
                return data;
            }
        }
    }
}
