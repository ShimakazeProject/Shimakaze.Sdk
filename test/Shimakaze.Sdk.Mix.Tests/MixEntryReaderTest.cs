
namespace Shimakaze.Sdk.Mix.Tests;

[TestClass]
public class MixEntryReaderTest
{
    private const string Assets = "Assets";
    private const string CsfFile = "ra2md.csf";
    private const string InputFile = "test.mix";
    private const uint Ra2mdCsf = 3179499641;

    [TestMethod]
    public void InitTest()
    {
        using MemoryStream ms = new([
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            0,
            0,

            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
        ]);
        ms.Seek(0, SeekOrigin.Begin);
        using MixEntryReader reader = new(ms);
        reader.Read();
    }

    [TestMethod]
    public async Task NotSupportAsyncTest()
    {
        await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            using MemoryStream ms = new([
                255,
                255,
                255,
                255,
                0,
                0,
                0,
                0,
                0,
                0,
            ]);
            await using MixEntryReader reader = new(ms);
            reader.Init();
        });
    }

    [TestMethod]
    public void NotSupportTest()
    {
        Assert.ThrowsException<NotImplementedException>(() =>
        {
            using MemoryStream ms = new([
                255,
                255,
                255,
                255,
                0,
                0,
                0,
                0,
                0,
                0,
            ]);
            using MixEntryReader reader = new(ms);
            reader.Init();
        });
    }

    [TestMethod]
    public void Test()
    {
        using Stream fs = File.OpenRead(Path.Combine(Assets, InputFile));
        using MixEntryReader reader = new(fs);
        reader.Init();
        Assert.AreEqual(4 + 2 + 4, fs.Position);
        Assert.AreNotEqual(0, reader.Count);
        Assert.AreNotEqual(0, reader.BodyOffset);

        MixEntry csf = default;
        for (int i = 0; i < reader.Count; i++)
        {
            var entry = reader.Read();
            Console.WriteLine(entry);
            Assert.AreEqual(4 + 2 + 4 + (i + 1) * 12, fs.Position);

            if (entry.Id is Ra2mdCsf)
                csf = entry;
        }

        Assert.ThrowsException<EndOfEntryTableException>(() => reader.Read());

        fs.Seek(reader.BodyOffset, SeekOrigin.Begin);
        Assert.AreEqual(reader.BodyOffset, fs.Position);

        fs.Seek(csf.Offset, SeekOrigin.Current);
        Assert.AreEqual(reader.BodyOffset + csf.Offset, fs.Position);
    }
}