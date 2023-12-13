using System.Collections;

using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.Csf.Tests;

[TestClass]
public class CsfSetTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputFile = "MergeTest.csf";
    private const string OutputPath = "Out";

    [TestMethod]
    public async Task BuildAndWriteToTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        await merger.BuildAndWriteToAsync(output, csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        output.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        output.Seek(0, SeekOrigin.Begin);

        while (stream.Position < stream.Length)
            Assert.AreEqual(stream.ReadByte(), output.ReadByte(), $"at {stream.Position}");
    }

    [TestMethod]
    public async Task BuildTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        CsfDocument newcsf = merger.Build(csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        Assert.AreEqual(csf.Metadata, newcsf.Metadata);
        Assert.IsTrue(newcsf.Data.SequenceEqual(csf.Data));
    }

    [TestMethod]
    public async Task ContainsTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.Contains(csf.Data.First()));
    }

    [TestMethod]
    public async Task CopyToTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        CsfData[] arr = new CsfData[csf.Data.Length];
        merger.CopyTo(arr, 0);
        Assert.IsTrue(merger.SetEquals(arr));

        arr = new CsfData[csf.Data.Length + 3];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3)));

        arr = new CsfData[csf.Data.Length + 6];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3).Take(csf.Data.Length)));
    }

    [TestMethod]
    public async Task ExceptWithTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.ExceptWith(csf.Data.Take(3));
        Assert.IsTrue(merger.SetEquals(csf.Data.Skip(3)));
    }

    [TestMethod]
    public async Task GetEnumeratorTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        using IEnumerator<CsfData> enumerator = merger.GetEnumerator();
        Assert.IsNotNull(enumerator);
        Assert.IsInstanceOfType<IEnumerator<CsfData>>(enumerator);
        Assert.IsInstanceOfType<IEnumerator>(enumerator);
    }

    [TestMethod]
    public async Task IntersectWithTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.IntersectWith(csf.Data.Take(3));
        Assert.IsTrue(merger.SetEquals(csf.Data.Take(3)));
    }

    [TestMethod]
    public async Task OverlapsTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.Overlaps(csf.Data));
        Assert.IsTrue(merger.Overlaps(csf.Data.Skip(1)));

        merger.Remove(csf.Data.Last());
        Assert.IsTrue(merger.Overlaps(csf.Data));
        Assert.IsTrue(merger.Overlaps(csf.Data.Skip(1)));
    }

    [TestMethod]
    public async Task RemoveTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.Remove(csf.Data.First());
        Assert.IsFalse(merger.SetEquals(csf.Data));
        Assert.IsTrue(merger.SetEquals(csf.Data.Skip(1)));
    }

    [TestMethod]
    public async Task SetEqualsTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.SetEquals(csf.Data));
        Assert.IsFalse(merger.SetEquals(csf.Data.Skip(1)));
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public async Task SubsetTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.IsSubsetOf(csf.Data));
        Assert.IsTrue(merger.IsSupersetOf(csf.Data));

        Assert.IsFalse(merger.IsProperSubsetOf(csf.Data));
        Assert.IsFalse(merger.IsProperSupersetOf(csf.Data));

        merger.Remove(csf.Data.First());
        Assert.IsTrue(merger.IsProperSubsetOf(csf.Data));
        Assert.IsTrue(merger.IsProperSupersetOf(csf.Data.Skip(1).Take(3)));
    }

    [TestMethod]
    public async Task SymmetricExceptWithTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.Remove(csf.Data.Last());
        merger.SymmetricExceptWith(csf.Data.Skip(1));
        Assert.IsTrue(merger.SetEquals(new[]
        {
            csf.Data.Last(),
            csf.Data.First(),
        }));
    }

    [TestMethod]
    public async Task UnionWithTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = await reader.ReadAsync();

        CsfSet merger = [];
        merger.UnionWith(csf.Data);
        Assert.AreNotEqual(0, merger.Count);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.Clear();
        Assert.AreEqual(0, merger.Count);
    }
}