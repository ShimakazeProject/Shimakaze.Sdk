using System.Collections.Concurrent;

namespace Shimakaze.Sdk.Models.Mix;

public class MixExpander
{
    private byte[] _buffer = new byte[6];

    /// <summary>
    /// 设置缓冲区大小
    /// </summary>
    /// <param name="size">大小</param>
    private void SetBufferSize(int size)
    {
        if (_buffer.Length < size)
            _buffer = new byte[size];
    }

    /// <summary>
    /// 释放文件
    /// </summary>
    /// <param name="input">输入流</param>
    /// <param name="output">输出目录</param>
    /// <param name="nameMapReader">列表的文本流</param>
    /// <param name="legacy">是否是传统Mix文件</param>
    /// <returns></returns>
    public async Task ExpandAsync(Stream input, DirectoryInfo output, TextReader? nameMapReader = null, bool legacy = false)
    {
        var info = await GetInfoAsync(input).ConfigureAwait(false);
        long bodyStartOffset = input.Position;
        Dictionary<uint, string> dictionary = await GetNameMap(input, info, bodyStartOffset, nameMapReader, legacy).ConfigureAwait(false);

        input.Seek(bodyStartOffset, SeekOrigin.Begin);
        await input.ReadAsync(_buffer.AsMemory(0, info.Header.BodySize)).ConfigureAwait(false);

        await Task.WhenAll(info.Entries.Select(entry => WriteToFile(output, entry, dictionary)));
    }

    /// <summary>
    /// 写入到文件
    /// </summary>
    /// <param name="output">输出目录</param>
    /// <param name="entry">Entry</param>
    /// <param name="dictionary">文件名映射表</param>
    /// <returns></returns>
    private async Task WriteToFile(DirectoryInfo output, MixIndexEntry entry, Dictionary<uint, string> dictionary)
    {
        string name = GetName(dictionary, entry.Id);
        FileStream file = File.Create(Path.Combine(output.FullName, name));
        await file.WriteAsync(_buffer.AsMemory(entry.Offset, entry.Size)).ConfigureAwait(false);
    }

    /// <summary>
    /// 从文件名映射表获取文件名
    /// </summary>
    /// <param name="dictionary">文件名映射表</param>
    /// <param name="id">ID</param>
    /// <returns>文件名</returns>
    private static string GetName(Dictionary<uint, string> dictionary, uint id)
    {
        if (!dictionary.TryGetValue(id, out string? name))
            name = $"0x{id:X8}";

        return name;
    }

    /// <summary>
    /// 获取名称映射
    /// </summary>
    /// <param name="input">输入流</param>
    /// <param name="info">Mix 文件头</param>
    /// <param name="bodyStartOffset">Mix主体部分偏移值</param>
    /// <param name="nameMapReader">列表的文本流</param>
    /// <param name="legacy">是否是传统Mix文件</param>
    /// <returns></returns>
    private async Task<Dictionary<uint, string>> GetNameMap(Stream input, MixInfo info, long bodyStartOffset, TextReader? nameMapReader = null, bool legacy = false)
    {
        ConcurrentDictionary<string, uint> nameMap = new();
        List<Task> tasks = new(2);
        if (nameMapReader is not null)
        {
            tasks.Add(GetNameMapFromListFileAsync(nameMap, nameMapReader));
        }

        tasks.Add(GetNameMapFromLocalXccDatabaseAsync(nameMap, input, info, bodyStartOffset, legacy));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return nameMap.ToDictionary(i => i.Value, i => i.Key);
    }

    /// <summary>
    /// 从 本地Xcc Database 中获取名称映射
    /// </summary>
    /// <param name="nameMap">线程安全的字典</param>
    /// <param name="input">输入流</param>
    /// <param name="info">Mix 文件头</param>
    /// <param name="bodyStartOffset">Mix主体部分偏移值</param>
    /// <param name="legacy">是否是传统Mix文件</param>
    /// <returns></returns>
    private async Task GetNameMapFromLocalXccDatabaseAsync(ConcurrentDictionary<string, uint> nameMap, Stream input, MixInfo info, long bodyStartOffset, bool legacy = false)
    {

        MixIndexEntry? localXccDatabase = info.Entries.FirstOrDefault(x => x.Id == (legacy ? Constants.LXD_TD_ID : Constants.LXD_TS_ID));

        if (localXccDatabase is not null)
        {
            IdCalculater idCalculater = legacy ? IdCalculaters.LegacyIdCalculater : IdCalculaters.IdCalculater;
            input.Seek(localXccDatabase.Offset + bodyStartOffset, SeekOrigin.Begin);
            input.Seek(48, SeekOrigin.Current); // Skip xcc local database summary.

            await input.ReadAsync(_buffer.AsMemory(0, 4)).ConfigureAwait(false);
            int count = BitConverter.ToInt32(_buffer, 0);

            StringBuilder builder = new();
            for (int i = 0; i < count; i++)
            {
                builder.Clear();
                int ch;
                while ((ch = input.ReadByte()) > 0)
                    builder.Append((char)ch);

                string name = builder.ToString();
                nameMap.AddOrUpdate(name, (name) => idCalculater(name), (name, id) => id);
            }
            input.Seek(bodyStartOffset, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// 从列表中读取名称映射
    /// </summary>
    /// <param name="nameMap">线程安全的字典</param>
    /// <param name="reader">文本流</param>
    /// <returns></returns>
    private static async Task GetNameMapFromListFileAsync(ConcurrentDictionary<string, uint> nameMap, TextReader reader)
    {
        Dictionary<uint, string> fileNameMap = new();
        while (reader.Peek() > 0)
        {
            string? line = await reader.ReadLineAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            string[] kvp = line.Split(":").Select(x => x.Trim()).ToArray();
            nameMap.AddOrUpdate(kvp[1], (name) => Convert.ToUInt32(kvp[0], 16), (name, id) => id);
        }
    }

    /// <summary>
    /// 获取 Mix 文件的信息
    /// </summary>
    /// <param name="input">输入文件流</param>
    /// <param name="buffer">缓冲区</param>
    /// <param name="legacy">是否是旧版MIX</param>
    /// <returns> 可以被等待的获取 <seealso cref="MixInfo"/> 的任务 </returns>
    /// <exception cref="NotSupportedException">当Mix被加密时抛出此异常</exception>
    private async Task<MixInfo> GetInfoAsync(Stream input, bool legacy = false)
    {
        const int entrySize = 12;
        const int idOffset = 0;
        const int startOffset = 4;
        const int sizeOffset = 8;
        uint? flag = null;
        if (!legacy)
        {
            await input.ReadAsync(_buffer.AsMemory(0, 4)).ConfigureAwait(false);
            flag = BitConverter.ToUInt32(_buffer, 0);
        }

        if ((flag & Constants.MIX_ENCRYPTED) != 0)
        {
            throw new NotSupportedException("This Mix File is Encrypted.");
        }

        await input.ReadAsync(_buffer.AsMemory(0, 6)).ConfigureAwait(false);
        short count = BitConverter.ToInt16(_buffer, 0);
        int bodySize = BitConverter.ToInt32(_buffer, 2);

        MixHeader header = new(flag, count, bodySize);
        var entriesSize = count * entrySize;
        SetBufferSize(Math.Max(header.BodySize, entriesSize));

        await input.ReadAsync(_buffer.AsMemory(0, entriesSize)).ConfigureAwait(false);

        MixIndexEntry[] entries = new MixIndexEntry[count];
        for (int i = 0; i < count; i++)
        {
            entries[i] = new(
                BitConverter.ToUInt32(_buffer, i * entrySize + idOffset),
                BitConverter.ToInt32(_buffer, i * entrySize + startOffset),
                BitConverter.ToInt32(_buffer, i * entrySize + sizeOffset)
            );
        }

        return new(header, entries);
    }

}
