using System.Runtime.InteropServices;
using System.Text;

using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf;

/// <summary>
/// Csf 写入器
/// </summary>
public sealed class CsfWriter : IWriter<CsfDocument>, IDisposable, IAsyncDisposable
{
    private readonly bool _leaveOpen;

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// 构造 Csf 写入器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    /// <exception cref="NotSupportedException">当流不支持Seek时抛出</exception>
    public CsfWriter(Stream baseStream, bool leaveOpen = false)
    {
        if (!baseStream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");

        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc/>
    public void Write(in CsfDocument value)
    {
        BaseStream.Write(value.Metadata);
        for (int i = 0; i < value.Data.Length; i++)
        {
            BaseStream.Write(value.Data[i].Identifier);
            BaseStream.Write(value.Data[i].StringCount);
            BaseStream.Write(value.Data[i].LabelNameLength);
            BaseStream.Write(value.Data[i].LabelName, value.Data[i].LabelNameLength);

            for (int j = 0; j < value.Data[i].Values.Length; j++)
            {
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
