using Shimakaze.Sdk.Csf.Yaml.Converter.V1;
using Shimakaze.Sdk.IO.Serialization;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Serialization;

/// <summary>
/// CSF YAML Serializer.
/// </summary>
public sealed class CsfYamlV1Serializer :
    ISerializer<CsfDocument?>,
    IDisposable, IAsyncDisposable
{

    private bool _disposedValue;
    private readonly bool _leaveOpen;

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="baseStream">������</param>
    /// <param name="leaveOpen">�˳�ʱ�Ƿ񱣳�����</param>
    public CsfYamlV1Serializer(Stream baseStream, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// ������
    /// </summary>
    public Stream BaseStream { get; }

    /// <inheritdoc/>
    public void Serialize(in CsfDocument? value)
    {
        using StreamWriter writer = new(BaseStream, leaveOpen: true);
        new SerializerBuilder()
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Serialize(writer, value);
    }

    /// <summary>
    /// �ͷ���Դ
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                    BaseStream.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// �첽�ͷź���
    /// </summary>
    /// <returns></returns>
    private async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }

    // ~CsfSerializer()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
    }
}