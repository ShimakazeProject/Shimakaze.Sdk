using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.IO.Csf.Serialization;

using System.Collections;

namespace Shimakaze.Sdk.IO.Csf;

[TestClass]
public class CsfMergerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputPath = "Out";
    private const string OutputFile = "MergeTest.csf";
    private const string OutputFileAsync = "MergeAsyncTest.csf";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void ContainsTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        Assert.IsTrue(merger.Contains(csf.First()));
    }

    [TestMethod]
    public void CopyToTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        CsfData[] arr = new CsfData[csf.Count];
        merger.CopyTo(arr, 0);
        Assert.IsTrue(merger.SetEquals(arr));

        arr = new CsfData[csf.Count + 3];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3)));

        arr = new CsfData[csf.Count + 6];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3).Take(csf.Count)));
    }

    [TestMethod]
    public void ExceptWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        merger.ExceptWith(csf.Take(3));
        Assert.IsTrue(merger.SetEquals(csf.Skip(3)));
    }

    [TestMethod]
    public void GetEnumeratorTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        using IEnumerator<CsfData> enumerator = merger.GetEnumerator();
        Assert.IsNotNull(enumerator);
        Assert.IsInstanceOfType<IEnumerator<CsfData>>(enumerator);
        Assert.IsInstanceOfType<IEnumerator>(enumerator);

    }

    [TestMethod]
    public void IntersectWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        merger.IntersectWith(csf.Take(3));
        Assert.IsTrue(merger.SetEquals(csf.Take(3)));
    }

    [TestMethod]
    public void SubsetTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        Assert.IsTrue(merger.IsSubsetOf(csf));
        Assert.IsTrue(merger.IsSupersetOf(csf));

        Assert.IsFalse(merger.IsProperSubsetOf(csf));
        Assert.IsFalse(merger.IsProperSupersetOf(csf));

        merger.Remove(csf.First());
        Assert.IsTrue(merger.IsProperSubsetOf(csf));
        Assert.IsTrue(merger.IsProperSupersetOf(csf.Skip(1).Take(3)));
    }

    [TestMethod]
    public void OverlapsTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        Assert.IsTrue(merger.Overlaps(csf));
        Assert.IsTrue(merger.Overlaps(csf.Skip(1)));

        merger.Remove(csf.Last());
        Assert.IsTrue(merger.Overlaps(csf));
        Assert.IsTrue(merger.Overlaps(csf.Skip(1)));
    }

    [TestMethod]
    public void RemoveTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        merger.Remove(csf.First());
        Assert.IsFalse(merger.SetEquals(csf));
        Assert.IsTrue(merger.SetEquals(csf.Skip(1)));
    }

    [TestMethod]
    public void SetEqualsTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        Assert.IsTrue(merger.SetEquals(csf));
        Assert.IsFalse(merger.SetEquals(csf.Skip(1)));
    }

    [TestMethod]
    public void SymmetricExceptWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        merger.Remove(csf.Last());
        merger.SymmetricExceptWith(csf.Skip(1));
        Assert.IsTrue(merger.SetEquals(new[]
        {
            csf.Last(),
            csf.First(),
        }));
    }

    [TestMethod]
    public void UnionWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.AreNotEqual(0, merger.Count);
        Assert.IsTrue(csf.SequenceEqual(merger));

        merger.Clear();
        Assert.AreEqual(0, merger.Count);
    }

    [TestMethod]
    public void BuildTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        CsfDocument newcsf = merger.Build(csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        Assert.AreEqual(csf.Metadata, newcsf.Metadata);
        Assert.IsTrue(newcsf.SequenceEqual(csf));
    }

    [TestMethod]
    public void BuildAndWriteToTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFileAsync));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        merger.BuildAndWriteTo(output, csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        output.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        output.Seek(0, SeekOrigin.Begin);

        while (stream.Position < stream.Length)
            Assert.AreEqual(stream.ReadByte(), output.ReadByte(), $"at {stream.Position}");
    }

    [TestMethod]
    public async Task BuildAndWriteToAsyncTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFileAsync));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();

        CsfMerger merger = new();
        merger.UnionWith(csf);
        Assert.IsTrue(csf.SequenceEqual(merger));

        await merger.BuildAndWriteToAsync(output, csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        await output.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        output.Seek(0, SeekOrigin.Begin);

        while (stream.Position < stream.Length)
            Assert.AreEqual(stream.ReadByte(), output.ReadByte(), $"at {stream.Position}");
    }
}