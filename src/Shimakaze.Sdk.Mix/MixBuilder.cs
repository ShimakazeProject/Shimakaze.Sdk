using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix 构建器
/// </summary>
/// <param name="idCalculater">ID计算器</param>
public sealed class MixBuilder(IdCalculater idCalculater)
{
    /// <summary>
    /// 将要被打包的文件
    /// </summary>
    public List<FileInfo> Files { get; } = [];

    /// <summary>
    /// 元数据
    /// </summary>
    public MixEntry[]? Entries { get; private set; }

    /// <summary>
    /// 当Entry被生成时触发
    /// </summary>
    public event EventHandler<(MixEntry Entry, FileInfo File)>? EntryGenerated;

    /// <summary>
    /// 准备写入Entries
    /// </summary>
    public event EventHandler<SetProgressEventArgs>? WriteEntries;

    /// <summary>
    /// 准备写入文件
    /// </summary>
    public event EventHandler<SetProgressEventArgs>? WriteFiles;

    /// <summary>
    /// 写入Entries完成
    /// </summary>
    public event EventHandler? WritedEntries;

    /// <summary>
    /// 写入文件完成
    /// </summary>
    public event EventHandler? WritedFiles;

    /// <summary>
    /// 构建MIX文件到流
    /// </summary>
    /// <param name="stream"> 流 </param>
    [MemberNotNull(nameof(Entries))]
    public async Task BuildAsync(Stream stream)
    {
        // 创建Entry写入器
        using MixEntryWriter writer = new(stream, true);
        int size = 0;
        // 获取若干Entry
        GetEntries(ref size);

        // 写入Metadata
        writer.WriteMetadataDirect((int)MixTag.NONE, new((short)Files.Count, size));

        // 写入若干Entry
        SetProgressEventArgs setProgress = new();
        WriteEntries?.Invoke(this, setProgress);
        for (int i = 0; i < Entries.Length; i++)
        {
            setProgress.Progress?.Report(i);
            writer.Write(Entries[i]);
        }
        WritedEntries?.Invoke(this, EventArgs.Empty);


        // 写入文件主体
        setProgress = new();
        WriteFiles?.Invoke(this, setProgress);
        for (int i = 0; i < Files.Count; i++)
        {
            setProgress.Progress?.Report(i);
            using var fs = Files[i].OpenRead();
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }
        WritedFiles?.Invoke(this, EventArgs.Empty);
    }

    [MemberNotNull(nameof(Entries))]
    private void GetEntries(ref int position)
    {
        ref FileInfo file = ref MemoryMarshal.GetReference(
            CollectionsMarshal.AsSpan(Files)
        );

        Entries = new MixEntry[Files.Count];
        ref MixEntry entry = ref MemoryMarshal.GetReference(Entries.AsSpan());
        for (int i = 0; i < Files.Count; i++)
        {
            ref var currentFile = ref Unsafe.Add(ref file, i);
            ref var currentEntry = ref Unsafe.Add(ref entry, i);
            currentEntry.Id = idCalculater(currentFile.Name.ToUpperInvariant());
            currentEntry.Offset = position;
            position += currentEntry.Size = (int)currentFile.Length;
            EntryGenerated?.Invoke(this, (currentEntry, currentFile));
        }
    }
}
