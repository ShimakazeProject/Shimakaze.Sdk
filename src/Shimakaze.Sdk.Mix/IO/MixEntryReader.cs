using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix Entry 读取器
/// </summary>
public sealed class MixEntryReader : AsyncReader<MixEntry>, IDisposable, IAsyncDisposable
{
    private bool _inited;

    /// <summary>
    /// 主体部分偏移位置
    /// </summary>
    public long BodyOffset { get; private set; }

    /// <summary>
    /// 主体部分文件大小
    /// </summary>
    public int BodySize { get; private set; }

    /// <summary>
    /// Entry的数量
    /// </summary>
    public short Count { get; private set; }

    /// <summary>
    /// 构造 Mix Entry 读取器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public MixEntryReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <exception cref="NotImplementedException"> 当Mix Entry被加密时抛出 </exception>
    public void Init()
    {
        // 标识符
        BaseStream.Read(out MixFlag flag);
        if ((flag & MixFlag.ENCRYPTED) is not 0)
            throw new NotImplementedException("This Mix File is Encrypted.");

        BaseStream.Read(out MixMetadata info);

        Count = info.Files;
        BodySize = info.Size;
        BodyOffset = BaseStream.Position + 12 * Count;

        _inited = true;
    }

    /// <summary>
    /// 读取一个Entry
    /// </summary>
    /// <returns> Entry </returns>
    /// <exception cref="EndOfEntryTableException"> 当没有可被读取的Entry时抛出 </exception>
    public MixEntry Read()
    {
        if (!_inited)
            Init();

        if (BaseStream.Position >= BodyOffset)
            throw new EndOfEntryTableException();

        BaseStream.Read(out MixEntry entry);
        return entry;
    }

    /// <inheritdoc />
    public override async Task<MixEntry> ReadAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return Read();
    }
}