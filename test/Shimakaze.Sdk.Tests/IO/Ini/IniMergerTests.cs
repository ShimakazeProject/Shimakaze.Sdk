using System.Collections;

using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.IO.Ini;

[TestClass]
public class IniMergerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "normal.ini";
    private const string OutputFile = "MergeTest.ini";
    private const string OutputFileAsync = "MergeAsyncTest.ini";
    private const string OutputPath = "Out";

    [TestMethod]
    public async Task BuildAndWriteToAsyncTest()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFileAsync));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        await merger.BuildAndWriteToAsync(output);
        await output.FlushAsync();
    }

    [TestMethod]
    public async Task BuildAndWriteToTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        await merger.BuildAndWriteToAsync(output);
        output.Flush();
    }

    [TestMethod]
    public async Task BuildTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        IniDocument newIni = merger.Build();
        Assert.IsFalse(newIni.Any<IniSection>(i => !ini.Contains(i)));
    }

    [TestMethod]
    public async Task ContainsTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        Assert.IsTrue(merger.Contains(ini.First<IniSection>()));
    }

    [TestMethod]
    public async Task CopyToTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

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
    public async Task ExceptWithTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.ExceptWith(ini.Take<IniSection>(3));
        Assert.IsTrue(merger.SetEquals(ini.Skip<IniSection>(3)));
    }

    [TestMethod]
    public async Task GetEnumeratorTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        using IEnumerator<IniSection> enumerator = merger.GetEnumerator();
        Assert.IsNotNull(enumerator);
        Assert.IsInstanceOfType<IEnumerator<IniSection>>(enumerator);
        Assert.IsInstanceOfType<IEnumerator>(enumerator);
    }

    [TestMethod]
    public async Task IntersectWithTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.IntersectWith(ini.Take<IniSection>(3));
        Assert.IsTrue(merger.SetEquals(ini.Take<IniSection>(3)));
    }

    [TestMethod]
    public async Task OverlapsTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

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
    public async Task RemoveTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.Remove(ini.First<IniSection>());
        Assert.IsFalse(merger.SetEquals(ini));
        Assert.IsTrue(merger.SetEquals(ini.Skip<IniSection>(1)));
    }

    [TestMethod]
    public async Task SetEqualsTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        Assert.IsTrue(merger.SetEquals(ini));
        Assert.IsFalse(merger.SetEquals(ini.Skip<IniSection>(1)));
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public async Task SubsetTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

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
    public async Task SymmetricExceptWithTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

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
    public async Task UnionWithTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        IniMerger merger = new();
        merger.UnionWith(ini);
        Assert.AreNotEqual(0, merger.Count);
        Assert.IsFalse(ini.Any<IniSection>(i => !merger.Contains(i)));

        merger.Clear();
        Assert.AreEqual(0, merger.Count);
    }
}