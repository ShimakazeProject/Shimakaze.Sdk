using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Mix;

[TestClass]
public class MixEntryReaderTest
{
    private const uint Ra2mdCsf = 3179499641;
    private const string Assets = "Assets";
    private const string InputFile = "test.mix";
    private const string CsfFile = "ra2md.csf";

    [TestMethod]
    public async Task Test()
    {
        await using Stream fs = File.OpenRead(Path.Combine(Assets, InputFile));
        using MixEntryReader reader = new(fs);
        reader.Init();
        Assert.AreEqual(4 + 2 + 4, fs.Position);
        Assert.AreNotEqual(0, reader.Count);
        Assert.AreNotEqual(0, reader.BodyOffset);

        MixEntry csf = default;
        for (int i = 0; i < reader.Count; i++)
        {
            var entry = i % 2 is 0 ? await reader.ReadAsync() : reader.Read();
            Console.WriteLine(entry);
            Assert.AreEqual(4 + 2 + 4 + (i + 1) * 12, fs.Position);

            if (entry.Id is Ra2mdCsf)
                csf = entry;
        }

        Assert.ThrowsException<EndOfEntryTableException>(() => reader.Read());
        await Assert.ThrowsExceptionAsync<EndOfEntryTableException>(async () => await reader.ReadAsync());

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

    [TestMethod]
    public void InitTest()
    {
        using MemoryStream ms = new(new byte[]{
            0, 0, 0, 0,
            1, 0,
            0, 0, 0, 0,

            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
        });
        ms.Seek(0, SeekOrigin.Begin);
        using MixEntryReader reader = new(ms);
        reader.Read();
    }

    [TestMethod]
    public void NotSupportTest()
    {
        Assert.ThrowsException<NotImplementedException>(() =>
        {
            using MemoryStream ms = new(new byte[]{
                255, 255, 255, 255,
                0, 0,
                0, 0, 0, 0,
            });
            using MixEntryReader reader = new(ms);
            reader.Init();
        });
    }
    [TestMethod]
    public async Task NotSupportAsyncTest()
    {
        await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await using MemoryStream ms = new(new byte[]{
                255, 255, 255, 255,
                0, 0,
                0, 0, 0, 0,
            });
            await using MixEntryReader reader = new(ms);
            await reader.InitAsync();
        });
    }
}