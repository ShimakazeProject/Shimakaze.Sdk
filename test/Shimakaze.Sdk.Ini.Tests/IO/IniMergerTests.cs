using System.Collections;

using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.IO.Ini.Serialization;

namespace Shimakaze.Sdk.IO.Ini;

[TestClass]
public class IniMergerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "normal.Ini";
    private const string OutputPath = "Out";
    private const string OutputFile = "MergeTest.Ini";
    private const string OutputFileAsync = "MergeAsyncTest.Ini";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void ContainsTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        Assert.IsTrue(merger.Contains(ini.First<IniSection>()));
    }

    [TestMethod]
    public void CopyToTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        IniSection[] arr = new IniSection[ini.Count];
        merger.CopyTo(arr, 0);
        Assert.IsTrue(merger.SetEquals(arr));

        arr = new IniSection[ini.Count + 3];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3)));

        arr = new IniSection[ini.Count + 6];
        merger.CopyTo(arr, 3);
        Assert.IsTrue(merger.SetEquals(arr.Skip(3).Take(ini.Count)));
    }

    [TestMethod]
    public void ExceptWithTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.ExceptWith(ini.Take<IniSection>(3));
        Assert.IsTrue(merger.SetEquals(ini.Skip<IniSection>(3)));
    }

    [TestMethod]
    public void GetEnumeratorTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        using IEnumerator<IniSection> enumerator = merger.GetEnumerator();
        Assert.IsNotNull(enumerator);
        Assert.IsInstanceOfType<IEnumerator<IniSection>>(enumerator);
        Assert.IsInstanceOfType<IEnumerator>(enumerator);

    }

    [TestMethod]
    public void IntersectWithTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.IntersectWith(ini.Take<IniSection>(3));
        Assert.IsTrue(merger.SetEquals(ini.Take<IniSection>(3)));
    }

    [TestMethod]
    public void SubsetTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        Assert.IsTrue(merger.IsSubsetOf(ini));
        Assert.IsTrue(merger.IsSupersetOf(ini));

        Assert.IsFalse(merger.IsProperSubsetOf(ini));
        Assert.IsFalse(merger.IsProperSupersetOf(ini));

        merger.Remove(ini.First<IniSection>());
        Assert.IsTrue(merger.IsProperSubsetOf(ini));
        Assert.IsTrue(merger.IsProperSupersetOf(ini.Skip<IniSection>(1).Take(3)));
    }

    [TestMethod]
    public void OverlapsTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        Assert.IsTrue(merger.Overlaps(ini));
        Assert.IsTrue(merger.Overlaps(ini.Skip<IniSection>(1)));

        merger.Remove(ini.Last<IniSection>());
        Assert.IsTrue(merger.Overlaps(ini));
        Assert.IsTrue(merger.Overlaps(ini.Skip<IniSection>(1)));
    }

    [TestMethod]
    public void RemoveTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.Remove(ini.First<IniSection>());
        Assert.IsFalse(merger.SetEquals(ini));
        Assert.IsTrue(merger.SetEquals(ini.Skip<IniSection>(1)));
    }

    [TestMethod]
    public void SetEqualsTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        Assert.IsTrue(merger.SetEquals(ini));
        Assert.IsFalse(merger.SetEquals(ini.Skip<IniSection>(1)));
    }

    [TestMethod]
    public void SymmetricExceptWithTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.Remove(ini.Last<IniSection>());
        merger.SymmetricExceptWith(ini.Skip<IniSection>(1));
        Assert.IsTrue(merger.SetEquals(new[]
        {
            ini.Last<IniSection>(),
            ini.First<IniSection>(),
        }));
    }

    [TestMethod]
    public void UnionWithTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.AreNotEqual(0, merger.Count);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.Clear();
        Assert.AreEqual(0, merger.Count);
    }

    [TestMethod]
    public void BuildTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        IniDocument newIni = merger.Build();
        Assert.IsFalse(newIni.Any<IniSection>(i => !ini.Contains(i)));
    }

    [TestMethod]
    public void BuildAndWriteToTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.BuildAndWriteTo(output);
        output.Flush();
    }

    [TestMethod]
    public async Task BuildAndWriteToAsyncTest()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFileAsync));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        await merger.BuildAndWriteToAsync(output);
        await output.FlushAsync();
    }
}