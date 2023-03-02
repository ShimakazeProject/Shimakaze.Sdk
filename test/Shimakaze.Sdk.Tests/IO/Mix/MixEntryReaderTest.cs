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
        using MixEntryReader reader = new(fs);
        Assert.AreEqual(fs.Position, 4 + 2 + 4);
        Assert.AreNotEqual(reader.Count, 0);
        Assert.AreNotEqual(reader.BodyOffset, 0);

        MixIndexEntry csf = default;
        for (int i = 0; i < reader.Count; i++)
        {
            var entry = await reader.ReadAsync();
            Console.WriteLine(entry);
            Assert.AreEqual(fs.Position, 4 + 2 + 4 + (i + 1) * 12);

            if (entry.Id is ra2md_csf)
                csf = entry;
        }
        fs.Seek(reader.BodyOffset, SeekOrigin.Begin);
        Assert.AreEqual(fs.Position, reader.BodyOffset);

        fs.Seek(csf.Offset, SeekOrigin.Current);
        Assert.AreEqual(fs.Position, reader.BodyOffset + csf.Offset);
        await using var ra2mdfs = File.OpenRead(Path.Combine(Assets, CsfFile));
        while (fs.Position < reader.BodyOffset + csf.Offset + csf.Size)
        {
            var _1 = fs.ReadByte();
            var _2 = ra2mdfs.ReadByte();
            Assert.AreEqual(_1, _2, $"At Position ${fs.Position}");
        }
    }
}