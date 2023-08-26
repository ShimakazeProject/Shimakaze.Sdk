using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf;

/// <summary>
/// Csf ��ȡ��
/// </summary>
public sealed class CsfReader : AsyncReader<CsfDocument>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// ���� Csf ��ȡ��
    /// </summary>
    /// <param name="stream"> ������ </param>
    /// <param name="leaveOpen"> �˳�ʱ�Ƿ񱣳����� </param>
    public CsfReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        CsfDocument csf = new();
        BaseStream.Read(out csf.Metadata);
        CsfThrowHelper.IsCsfFile(csf.Metadata.Identifier);
        csf.Data = new CsfData[csf.Metadata.LabelCount];

        await Task.Yield();

        for (int i = 0; i < csf.Metadata.LabelCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / csf.Data.Length);

            BaseStream.Read(out csf.Data[i].Identifier);
            CsfThrowHelper.IsLabel(csf.Data[i].Identifier, () => new object[] { i, BaseStream.Position });
            BaseStream.Read(out csf.Data[i].StringCount);
            BaseStream.Read(out csf.Data[i].LabelNameLength);
            BaseStream.Read(out csf.Data[i].LabelName, csf.Data[i].LabelNameLength);

            csf.Data[i].Values = new CsfValue[csf.Data[i].StringCount];
            for (int j = 0; j < csf.Data[i].StringCount; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                BaseStream.Read(out csf.Data[i].Values[j].Identifier);
                CsfThrowHelper.IsStringOrExtraString(csf.Data[i].Values[j].Identifier, () => new object[] { i, j, BaseStream.Position });

                BaseStream.Read(out csf.Data[i].Values[j].ValueLength);
                BaseStream.Read(out csf.Data[i].Values[j].Value, csf.Data[i].Values[j].ValueLength, true);
                unsafe
                {
                    fixed (char* ptr = csf.Data[i].Values[j].Value)
                        CsfConstants.CodingValue((byte*)ptr, csf.Data[i].Values[j].ValueLength * sizeof(char));
                }

                if (csf.Data[i].Values[j].HasExtra)
                {
                    BaseStream.Read(out int length);
                    csf.Data[i].Values[j].ExtraValueLength = length;
                    BaseStream.Read(out csf.Data[i].Values[j].ExtraValue, length);
                }
            }
        }

        return csf;
    }
}