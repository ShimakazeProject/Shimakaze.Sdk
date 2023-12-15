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

    private CsfDocument _csf = default!;

    [TestInitialize]
    public async Task Initialize()
    {
        Directory.CreateDirectory(OutputPath);
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        _csf = await reader.ReadAsync();
    }

    [TestMethod]
    public async Task BuildAndWriteToTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFile));

        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        await merger.BuildAndWriteToAsync(output, _csf.Metadata.Language, _csf.Metadata.Version, _csf.Metadata.Unknown);
        output.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        output.Seek(0, SeekOrigin.Begin);

        while (stream.Position < stream.Length)
            Assert.AreEqual(stream.ReadByte(), output.ReadByte(), $"at {stream.Position}");
    }

    [TestMethod]
    public void BuildTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        CsfDocument newcsf = merger.Build(_csf.Metadata.Language, _csf.Metadata.Version, _csf.Metadata.Unknown);
        Assert.AreEqual(_csf.Metadata, newcsf.Metadata);
        Assert.IsTrue(newcsf.Data.SequenceEqual(_csf.Data));
    }

    [TestMethod]
    public void ContainsTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.Contains(_csf.Data.First()));
    }

    [TestMethod]
    public void CopyToTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        CsfData[] arr = new CsfData[_csf.Data.Length];
        merger.CopyTo(arr, 0);
        Assert.IsTrue(merger.SetEquals(arr));

        arr = new CsfData[_csf.Data.Length + 3];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3)));

        arr = new CsfData[_csf.Data.Length + 6];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3).Take(_csf.Data.Length)));
    }

    [TestMethod]
    public void ExceptWithTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        merger.ExceptWith(_csf.Data.Take(3));
        Assert.IsTrue(merger.SetEquals(_csf.Data.Skip(3)));
    }

    [TestMethod]
    public void GetEnumeratorTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        using IEnumerator<CsfData> enumerator = merger.GetEnumerator();
        Assert.IsNotNull(enumerator);
        Assert.IsInstanceOfType<IEnumerator<CsfData>>(enumerator);
        Assert.IsInstanceOfType<IEnumerator>(enumerator);
    }

    [TestMethod]
    public void IntersectWithTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        merger.IntersectWith(_csf.Data.Take(3));
        Assert.IsTrue(merger.SetEquals(_csf.Data.Take(3)));
    }

    [TestMethod]
    public void OverlapsTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.Overlaps(_csf.Data));
        Assert.IsTrue(merger.Overlaps(_csf.Data.Skip(1)));

        merger.Remove(_csf.Data.Last());
        Assert.IsTrue(merger.Overlaps(_csf.Data));
        Assert.IsTrue(merger.Overlaps(_csf.Data.Skip(1)));
    }

    [TestMethod]
    public void RemoveTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        merger.Remove(_csf.Data.First());
        Assert.IsFalse(merger.SetEquals(_csf.Data));
        Assert.IsTrue(merger.SetEquals(_csf.Data.Skip(1)));
    }

    [TestMethod]
    public void SetEqualsTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.SetEquals(_csf.Data));
        Assert.IsFalse(merger.SetEquals(_csf.Data.Skip(1)));
    }


    [TestMethod]
    public void SubsetTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        Assert.IsTrue(merger.IsSubsetOf(_csf.Data));
        Assert.IsTrue(merger.IsSupersetOf(_csf.Data));

        Assert.IsFalse(merger.IsProperSubsetOf(_csf.Data));
        Assert.IsFalse(merger.IsProperSupersetOf(_csf.Data));

        merger.Remove(_csf.Data.First());
        Assert.IsTrue(merger.IsProperSubsetOf(_csf.Data));
        Assert.IsTrue(merger.IsProperSupersetOf(_csf.Data.Skip(1).Take(3)));
    }

    [TestMethod]
    public void SymmetricExceptWithTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        merger.Remove(_csf.Data.Last());
        merger.SymmetricExceptWith(_csf.Data.Skip(1));
        Assert.IsTrue(merger.SetEquals(new[]
        {
            _csf.Data.Last(),
            _csf.Data.First(),
        }));
    }

    [TestMethod]
    public void UnionWithTest()
    {
        CsfSet merger = [];
        merger.UnionWith(_csf.Data);
        Assert.AreNotEqual(0, merger.Count);
        Assert.IsTrue(_csf.Data.SequenceEqual(merger));

        merger.Clear();
        Assert.AreEqual(0, merger.Count);
    }
}