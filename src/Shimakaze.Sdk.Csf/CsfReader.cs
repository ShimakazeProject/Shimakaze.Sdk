namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf ¶ÁÈ¡Æ÷
/// </summary>
public sealed class CsfReader
{
    /// <summary>
    /// Csf ¶ÁÈ¡Æ÷
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public static CsfDocument Read(Stream stream, IProgress<float>? progress = default)
    {
        CsfDocument csf = new();
        stream.Read(out csf.InternalMetadata);
        CsfAsserts.IsCsfFile(csf.Metadata.Identifier);
        csf.Data = new CsfData[csf.Metadata.LabelCount];

        for (int i = 0; i < csf.Metadata.LabelCount; i++)
        {
            csf.Data[i] ??= new();
            progress?.Report((float)i / csf.Data.Length);

            stream.Read(out csf.Data[i].InternalIdentifier);
            CsfAsserts.IsLabel(csf.Data[i].Identifier, () => new object[] { i, stream.Position });
            stream.Read(out csf.Data[i].InternalStringCount);
            stream.Read(out csf.Data[i].InternalLabelNameLength);
            stream.Read(out csf.Data[i].InternalLabelName, csf.Data[i].LabelNameLength);

            csf.Data[i].Values = new CsfValue[csf.Data[i].StringCount];
            for (int j = 0; j < csf.Data[i].StringCount; j++)
            {
                stream.Read(out csf.Data[i].Values[j].InternalIdentifier);
                CsfAsserts.IsStringOrExtraString(csf.Data[i].Values[j].Identifier, () => new object[] { i, j, stream.Position });

                stream.Read(out csf.Data[i].Values[j].InternalValueLength);
                stream.Read(out csf.Data[i].Values[j].InternalValue, csf.Data[i].Values[j].ValueLength, true);
                unsafe
                {
                    fixed (char* ptr = csf.Data[i].Values[j].Value)
                        CsfConstants.CodingValue((byte*)ptr, csf.Data[i].Values[j].ValueLength * sizeof(char));
                }

                if (csf.Data[i].Values[j].HasExtra)
                {
                    stream.Read(out int length);
                    csf.Data[i].Values[j].ExtraValueLength = length;
                    stream.Read(out csf.Data[i].Values[j].InternalExtraValue, length);
                }
            }
        }

        return csf;
    }
}