using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix 构建器
/// </summary>
public class MixBuilder
{
    /// <summary>
    /// 将要被打包的文件
    /// </summary>
    protected readonly List<FileInfo> _files = new();
    /// <summary>
    /// ID计算器
    /// </summary>
    public required IdCalculater IdCalculater { get; init; }

    /// <summary>
    /// 已被添加到构建器的文件的个数
    /// </summary>
    public int FileCount => _files.Count;

    /// <summary>
    /// 添加一个文件到构建器
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>构建器</returns>
    public MixBuilder AddFile(FileInfo file)
    {
        _files.Add(file);
        return this;
    }

    /// <summary>
    /// 从构建器移除一个文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>构建器</returns>
    public MixBuilder RemoveFile(FileInfo file)
    {
        _files.Remove(file);
        return this;
    }

    /// <summary>
    /// 构建MIX文件到流
    /// </summary>
    /// <param name="stream">流</param>
    public virtual async Task BuildAsync(Stream stream)
    {
        // 创建Entry写入器
        await using MixEntryWriter writer = new(stream, true);
        int size = 0;
        // 获取若干Entry
        MixEntry[] entries = GetEntries(ref size);

        // 写入Metadata
        writer.WriteMetadataDirect((int)MixFlag.NONE, new((short)_files.Count, size));

        // 写入若干Entry
        foreach (var entry in entries)
            writer.Write(entry);

        // 写入文件主体
        foreach (var file in _files)
        {
            await using var fs = file.OpenRead();
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }
    }

    private MixEntry[] GetEntries(ref int position)
    {
        ref FileInfo file = ref MemoryMarshal.GetReference(
            CollectionsMarshal.AsSpan(_files)
        );

        MixEntry[] entries = new MixEntry[_files.Count];
        ref MixEntry entry = ref MemoryMarshal.GetReference(entries.AsSpan());
        for (int i = 0; i < _files.Count; i++)
        {
            ref var currentFile = ref Unsafe.Add(ref file, i);
            ref var currentEntry = ref Unsafe.Add(ref entry, i);
            currentEntry.Id = IdCalculater(currentFile.Name.ToUpper());
            currentEntry.Offset = position;
            position += currentEntry.Size = (int)currentFile.Length;
        }

        return entries;
    }
}