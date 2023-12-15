﻿namespace Shimakaze.Sdk.Csf.Tests;

[TestClass]
public class CsfReaderTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";

    [TestMethod]
    public async Task ReadTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        Assert.IsNotNull(await reader.ReadAsync());
    }
}