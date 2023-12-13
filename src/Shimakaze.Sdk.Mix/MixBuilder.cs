using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix 构建器
/// </summary>
public class MixBuilder
{
    /// <summary>
    /// 将要被打包的文件
    /// </summary>
    protected List<FileInfo> Files { get; } = [];

    /// <summary>
    /// 已被添加到构建器的文件的个数
    /// </summary>
    public int FileCount => Files.Count;

    /// <summary>
    /// ID计算器
    /// </summary>
    public required IdCalculater IdCalculater { get; init; }

    /// <summary>
    /// 添加一个文件到构建器
    /// </summary>
    /// <param name="file"> 文件 </param>
    /// <returns> 构建器 </returns>
    public MixBuilder AddFile(FileInfo file)
    {
        Files.Add(file);
        return this;
    }

    /// <summary>
    /// 构建MIX文件到流
    /// </summary>
    /// <param name="stream"> 流 </param>
    public virtual async Task BuildAsync(Stream stream)
    {
        // 创建Entry写入器
        using MixEntryWriter writer = new(stream, true);
        int size = 0;
        // 获取若干Entry
        MixEntry[] entries = GetEntries(ref size);

        // 写入Metadata
        writer.WriteMetadataDirect((int)MixTag.NONE, new((short)Files.Count, size));

        // 写入若干Entry
        foreach (var entry in entries)
            writer.Write(entry);

        // 写入文件主体
        foreach (var file in Files)
        {
            using var fs = file.OpenRead();
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 从构建器移除一个文件
    /// </summary>
    /// <param name="file"> 文件 </param>
    /// <returns> 构建器 </returns>
    public MixBuilder RemoveFile(FileInfo file)
    {
        Files.Remove(file);
        return this;
    }

    private MixEntry[] GetEntries(ref int position)
    {
        ref FileInfo file = ref MemoryMarshal.GetReference(
            CollectionsMarshal.AsSpan(Files)
        );

        MixEntry[] entries = new MixEntry[Files.Count];
        ref MixEntry entry = ref MemoryMarshal.GetReference(entries.AsSpan());
        for (int i = 0; i < Files.Count; i++)
        {
            ref var currentFile = ref Unsafe.Add(ref file, i);
            ref var currentEntry = ref Unsafe.Add(ref entry, i);
            currentEntry.Id = IdCalculater(currentFile.Name.ToUpperInvariant());
            currentEntry.Offset = position;
            position += currentEntry.Size = (int)currentFile.Length;
        }

        return entries;
    }
}
