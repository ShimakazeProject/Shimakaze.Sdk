using Shimakaze.Sdk.Data.Mix;
using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Tests.IO.Mix;

[TestClass()]
public class MixEntryReaderTest
{
    private const uint ra2md_csf = 3179499641;
    private const string Assets = "Assets";
    private const string InputFile = "test.mix";
    private const string CsfFile = "ra2md.csf";

    [TestMethod()]
    public async Task Test()
    {
        await using Stream fs = File.OpenRead(Path.Combine(Assets, InputFile));
        using MixEntryReader reader = await MixEntryReader.CreateAsync(fs);
        Assert.AreEqual(4 + 2 + 4, fs.Position);
        Assert.AreNotEqual(0, reader.Count);
        Assert.AreNotEqual(0, reader.BodyOffset);

        MixIndexEntry csf = default;
        for (int i = 0; i < reader.Count; i++)
        {
            var entry = await reader.ReadAsync();
            Console.WriteLine(entry);
            Assert.AreEqual(4 + 2 + 4 + (i + 1) * 12, fs.Position);

            if (entry.Id is ra2md_csf)
                csf = entry;
        }
        fs.Seek(reader.BodyOffset, SeekOrigin.Begin);
        Assert.AreEqual(reader.BodyOffset, fs.Position);

        fs.Seek(csf.Offset, SeekOrigin.Current);
        Assert.AreEqual(reader.BodyOffset + csf.Offset, fs.Position);
        await using var ra2mdfs = File.OpenRead(Path.Combine(Assets, CsfFile));
        while (fs.Position < reader.BodyOffset + csf.Offset + csf.Size)
        {
            var _1 = fs.ReadByte();
            var _2 = ra2mdfs.ReadByte();
            Assert.AreEqual(_2, _1, $"At Position {fs.Position}");
        }
    }
}