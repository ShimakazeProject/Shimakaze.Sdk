using System.Text;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf 写入器
/// </summary>
public static class CsfWriter
{
    /// <summary>
    /// 写入CSF数据到流
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <param name="progress"></param>
    public static void Write(Stream stream, CsfDocument value, IProgress<float>? progress = default)
    {
        stream.Write(value.Metadata);

        for (int i = 0; i < value.Data.Length; i++)
        {
            progress?.Report((float)i / value.Data.Length);

            stream.Write(value.Data[i].Identifier);
            stream.Write(value.Data[i].StringCount);
            stream.Write(value.Data[i].LabelNameLength);
            stream.Write(value.Data[i].LabelName, value.Data[i].LabelNameLength);

            for (int j = 0; j < value.Data[i].Values.Length; j++)
            {
                stream.Write(value.Data[i].Values[j].Identifier);
                stream.Write(value.Data[i].Values[j].ValueLength);
                byte[] str = Encoding.Unicode.GetBytes(value.Data[i].Values[j].Value);
                str = CsfConstants.CodingValue(str);
                stream.Write(str, 0, value.Data[i].Values[j].ValueLength << 1);

                if (value.Data[i].Values[j] is
                    {
                        HasExtra: true,
                        ExtraValue: not null
                    } e)
                {
                    stream.Write(e.ExtraValueLength.Value);
                    stream.Write(e.ExtraValue, e.ExtraValueLength.Value);
                }
            }
        }
    }
}
