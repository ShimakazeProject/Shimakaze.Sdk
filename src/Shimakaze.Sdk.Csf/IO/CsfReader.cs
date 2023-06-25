using Shimakaze.Sdk.Csf;


namespace Shimakaze.Sdk.IO.Csf;

/// <summary>
/// Csf 读取器
/// </summary>
public sealed class CsfReader : IReader<CsfDocument>, IDisposable, IAsyncDisposable
{
    private readonly bool _leaveOpen;

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// 构造 Csf 读取器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    public CsfReader(Stream baseStream, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc/>
    public CsfDocument Read()
    {
        CsfDocument csf = new();
        BaseStream.Read(out csf.Metadata);
        CsfThrowHelper.IsCsfFile(csf.Metadata.Identifier);
        csf.Data = new CsfData[csf.Metadata.LabelCount];

        for (int i = 0; i < csf.Metadata.LabelCount; i++)
        {
            BaseStream.Read(out csf.Data[i].Identifier);
            CsfThrowHelper.IsLabel(csf.Data[i].Identifier, () => new object[] { i, BaseStream.Position });
            BaseStream.Read(out csf.Data[i].StringCount);
            BaseStream.Read(out csf.Data[i].LabelNameLength);
            BaseStream.Read(out csf.Data[i].LabelName, csf.Data[i].LabelNameLength);

            csf.Data[i].Values = new CsfValue[csf.Data[i].StringCount];
            for (int j = 0; j < csf.Data[i].StringCount; j++)
            {
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_leaveOpen)
            BaseStream.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }
}