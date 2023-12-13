namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf 写入器
/// </summary>
public sealed class CsfWriter : AsyncWriter<CsfDocument>, ICsfWriter
{
    /// <summary>
    /// 构造 Csf 写入器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    /// <exception cref="NotSupportedException"> 当流不支持Seek时抛出 </exception>
    public CsfWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        if (!stream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");
    }

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        BaseStream.Write(value.Metadata);

        await Task.Yield();

        for (int i = 0; i < value.Data.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.Data.Length);

            BaseStream.Write(value.Data[i].Identifier);
            BaseStream.Write(value.Data[i].StringCount);
            BaseStream.Write(value.Data[i].LabelNameLength);
            BaseStream.Write(value.Data[i].LabelName, value.Data[i].LabelNameLength);

            for (int j = 0; j < value.Data[i].Values.Length; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                BaseStream.Write(value.Data[i].Values[j].Identifier);
                BaseStream.Write(value.Data[i].Values[j].ValueLength);
                unsafe
                {
                    fixed (char* ptr = value.Data[i].Values[j].Value)
                        CsfConstants.CodingValue((byte*)ptr, value.Data[i].Values[j].ValueLength * sizeof(char));
                }
                BaseStream.Write(value.Data[i].Values[j].Value, value.Data[i].Values[j].ValueLength, true);

                if (value.Data[i].Values[j] is
                    {
                        HasExtra: true,
                        ExtraValue: not null
                    } e)
                {
                    BaseStream.Write(e.ExtraValueLength.Value);
                    BaseStream.Write(e.ExtraValue, e.ExtraValueLength.Value);
                }
            }
        }
    }
}