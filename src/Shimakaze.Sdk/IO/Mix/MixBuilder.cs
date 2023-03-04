
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Data.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix 构建器
/// </summary>
public class MixBuilder
{
    /// <summary>
    /// 将要被打包的文件
    /// </summary>
    protected readonly List<FileInfo> files = new();
    /// <summary>
    /// ID计算器
    /// </summary>
    protected IdCalculater? idCalculater;
    /// <summary>
    /// 添加一个文件到构建器
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>构建器</returns>
    public MixBuilder AddFile(FileInfo file)
    {
        files.Add(file);
        return this;
    }

    /// <summary>
    /// 从构建器移除一个文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>构建器</returns>
    public MixBuilder RemoveFile(FileInfo file)
    {
        files.Remove(file);
        return this;
    }

    /// <summary>
    /// 设置构建器使用的ID计算器
    /// </summary>
    /// <param name="idCalculater">ID计算器</param>
    /// <returns>构建器</returns>
    public MixBuilder SetIdCaculater(IdCalculater idCalculater)
    {
        this.idCalculater = idCalculater;
        return this;
    }

    /// <summary>
    /// 构建MIX文件到流
    /// </summary>
    /// <param name="stream">流</param>
    public virtual async Task BuildAsync(Stream stream)
    {
        // 创建Entry写入器
        using MixEntryWriter writer = await MixEntryWriter.CreateAsync(stream, true);
        int size = 0;
        // 写入若干Entry
        foreach (var entry in GetEntries(ref size))
            await writer.WriteAsync(entry).ConfigureAwait(false);
        // 写入文件主体
        foreach (var file in files)
        {
            await using var fs = file.OpenRead();
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }

        // 写入Metadata
        await writer.WriteCount((short)files.Count).ConfigureAwait(false);
        await writer.WriteBodySize(size).ConfigureAwait(false);
    }

    private MixEntry[] GetEntries(ref int position)
    {
        if (idCalculater is null)
            throw new Exception($"{nameof(idCalculater)} MUST be set!");

        ref FileInfo file = ref MemoryMarshal.GetReference(
            CollectionsMarshal.AsSpan(files)
        );

        MixEntry[] entries = new MixEntry[files.Count];
        ref MixEntry entry = ref MemoryMarshal.GetReference(entries.AsSpan());
        for (int i = 0; i < files.Count; i++)
        {
            ref var currentFile = ref Unsafe.Add(ref file, i);
            ref var currentEntry = ref Unsafe.Add(ref entry, i);
            currentEntry.Id = idCalculater(currentFile.Name.ToUpper());
            currentEntry.Offset = position;
            position += currentEntry.Size = (int)currentFile.Length;
        }

        return entries;
    }
}