using System.Text;

using Shimakaze.Sdk.Data.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix 拆解器
/// </summary>
public class MixExpander
{
    private Stream Input { get; init; }
    private string OutputPath { get; init; }
    private bool NoFlag { get; set; }
    private readonly byte[] buffer;
    private readonly TextReader? nameMapReader;

    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="input"></param>
    /// <param name="outputPath"></param>
    /// <param name="buffer"></param>
    /// <param name="nameMapReader"></param>
    /// <param name="noFlag"></param>
    public MixExpander(Stream input, string outputPath, byte[] buffer, TextReader? nameMapReader = default, bool noFlag = false)
    {
        Input = input;
        OutputPath = outputPath;
        NoFlag = noFlag;
        this.nameMapReader = nameMapReader;
        this.buffer = buffer;
    }

    /// <summary>
    /// 拆解
    /// </summary>
    /// <returns></returns>
    public async Task Expand()
    {
        using BinaryReader mix = new(Input, Encoding.ASCII, true);

        var (mixHead, mixEntries) = HeaderParser(mix);

        var body_offset = mix.BaseStream.Position;

        var nameMap = await NameMapParserAsync(
            mix.BaseStream,
            mixEntries.Any(x => x.Id is Constants.LXD_TS_ID or Constants.LXD_TD_ID)
                    ? mixEntries.First(x => x.Id is Constants.LXD_TS_ID or Constants.LXD_TD_ID)
                    : null,
            body_offset);

        int num = 0;
        Console.WriteLine("Expanding...");
        Console.WriteLine("==============================================================");
        Console.WriteLine("    No    |     ID     |   Offset   |    Size    |    Name    ");
        foreach (var entry in mixEntries)
        {
            num++;
            var name = GetName(entry.Id);
            Console.WriteLine($" {num:D8} | 0x{entry.Id:X8} | 0x{entry.Offset:X8} | 0x{entry.Size:X8} | {name}");
            using var file = File.Create(Path.Combine(OutputPath, name));
            Input.Seek(entry.Offset + body_offset, SeekOrigin.Begin);
            buffer.CheckLength(entry.Size);
            Input.Read(buffer.AsSpan(0, entry.Size));
            file.Write(buffer.AsSpan(0, entry.Size));
        }
        Console.WriteLine("==============================================================");
        Console.WriteLine("All Done!");

        string GetName(uint id) => nameMap is not null && nameMap.TryGetValue(id, out var name) ? name : $"0x{id:X8}";
    }


    private (MixMetadata mixHead, MixEntry[] mixEntries) HeaderParser(BinaryReader br)
    {
        MixMetadata mixHead;
        MixEntry[] mixEntries;

        uint mixFlag = NoFlag ? (uint)MixFlag.NONE : br.ReadUInt32();
        if ((mixFlag & (uint)MixFlag.ENCRYPTED) != 0)
        {
            throw new NotImplementedException("This Mix File is Encrypted.");
        }
        else
        {
            mixHead = new()
            {
                Files = br.ReadInt16(),
                Size = br.ReadInt32()
            };
            mixEntries = new MixEntry[mixHead.Files];
            for (int i = 0; i < mixHead.Files; i++)
            {
                mixEntries[i] = new()
                {
                    Id = br.ReadUInt32(),
                    Offset = br.ReadInt32(),
                    Size = br.ReadInt32()
                };
            }
        }

        return (mixHead, mixEntries!);
    }


    private async Task<Dictionary<uint, string>> NameMapParserAsync(Stream stream, MixEntry? lxd, long body_offset)
    {
        Dictionary<uint, string> fileNameMap = new();
        Task<Dictionary<uint, string>>? tLnmf = default;
        Task<Dictionary<uint, string>>? tLxd = default;

        if (nameMapReader is not null)
        {
            tLnmf = Task.Run(() =>
            {
                Console.WriteLine("Loading FileMap List");
                Dictionary<uint, string> fileNameMap = new();
                while (nameMapReader.Peek() > 0)
                {
                    var line = nameMapReader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var kvp = line.Split(":").Select(x => x.Trim()).ToArray();
                    fileNameMap.Add(Convert.ToUInt32(kvp[0], 16), kvp[1].Split("#")[0]);
                }
                return fileNameMap;
            });
        }
        if (lxd is not null)
        {
            tLxd = Task.Run(() =>
            {
                Dictionary<uint, string> fileNameMap = new();
                try
                {
                    fileNameMap.Add(lxd.Value.Id, "xcc local database.dat");

                    Func<string, uint> GetId = lxd.Value.Id is Constants.LXD_TS_ID
                        ? IdCalculaters.TSIdCalculater
                        : IdCalculaters.OldIdCalculater;

                    Console.WriteLine("Find local xcc database.dat File!");
                    Console.WriteLine("Trying Generat FileName Map.");

                    StringBuilder sb = new();
                    using BinaryReader br = new(stream, Encoding.ASCII, true);
                    stream.Seek(lxd.Value.Offset + body_offset, SeekOrigin.Begin);
                    stream.Seek(48, SeekOrigin.Current);

                    var count = br.ReadInt32();

                    byte ch;
                    for (int i = 0; i < count; i++)
                    {
                        if (stream.Position > lxd.Value.Offset + body_offset + lxd.Value.Size)
                            throw new FileLoadException("Cannot Load Filename from " + fileNameMap[lxd.Value.Id]);

                        sb.Clear();
                        while ((ch = br.ReadByte()) > 0)
                            sb.Append((char)ch);
                        var name = sb.ToString();
                        fileNameMap[GetId(name)] = name;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                return fileNameMap;
            });


        }

        foreach (var x in (
            await Task.WhenAll(new[] { tLxd, tLnmf }.Where(i => i is not null).Select(i => i!))
            ).SelectMany(i => i))
        {
            fileNameMap[x.Key] = x.Value;
        }
        return fileNameMap;
    }
}
