using Shimakaze.Sdk.Csf;

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
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.Contains(csf.Data.First()));
    }

    [TestMethod]
    public void CopyToTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
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
    public void ExceptWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.ExceptWith(csf.Data.Take(3));
        Assert.IsTrue(merger.SetEquals(csf.Data.Skip(3)));
    }

    [TestMethod]
    public void GetEnumeratorTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        using IEnumerator<CsfData> enumerator = merger.GetEnumerator();
        Assert.IsNotNull(enumerator);
        Assert.IsInstanceOfType<IEnumerator<CsfData>>(enumerator);
        Assert.IsInstanceOfType<IEnumerator>(enumerator);

    }

    [TestMethod]
    public void IntersectWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.IntersectWith(csf.Data.Take(3));
        Assert.IsTrue(merger.SetEquals(csf.Data.Take(3)));
    }

    [TestMethod]
    public void SubsetTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
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
    public void OverlapsTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.Overlaps(csf.Data));
        Assert.IsTrue(merger.Overlaps(csf.Data.Skip(1)));

        merger.Remove(csf.Data.Last());
        Assert.IsTrue(merger.Overlaps(csf.Data));
        Assert.IsTrue(merger.Overlaps(csf.Data.Skip(1)));
    }

    [TestMethod]
    public void RemoveTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.Remove(csf.Data.First());
        Assert.IsFalse(merger.SetEquals(csf.Data));
        Assert.IsTrue(merger.SetEquals(csf.Data.Skip(1)));
    }

    [TestMethod]
    public void SetEqualsTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.SetEquals(csf.Data));
        Assert.IsFalse(merger.SetEquals(csf.Data.Skip(1)));
    }

    [TestMethod]
    public void SymmetricExceptWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
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
    public void UnionWithTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.AreNotEqual(0, merger.Count);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.Clear();
        Assert.AreEqual(0, merger.Count);
    }

    [TestMethod]
    public void BuildTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        CsfDocument newcsf = merger.Build(csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        Assert.AreEqual(csf.Metadata, newcsf.Metadata);
        Assert.IsTrue(newcsf.Data.SequenceEqual(csf.Data));
    }

    [TestMethod]
    public void BuildAndWriteToTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFileAsync));
        using CsfReader reader = new(stream);
        CsfDocument csf = reader.Read();

        CsfMerger merger = new();
        merger.UnionWith(csf.Data);
        Assert.IsTrue(csf.Data.SequenceEqual(merger));

        merger.BuildAndWriteTo(output, csf.Metadata.Language, csf.Metadata.Version, csf.Metadata.Unknown);
        output.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        output.Seek(0, SeekOrigin.Begin);

        while (stream.Position < stream.Length)
            Assert.AreEqual(stream.ReadByte(), output.ReadByte(), $"at {stream.Position}");
    }
}