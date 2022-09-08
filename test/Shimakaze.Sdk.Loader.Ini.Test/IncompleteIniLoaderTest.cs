namespace Shimakaze.Sdk.Loader.Ini.Test;

[TestClass]
public class IncompleteIniLoaderTest
{
    [TestMethod]
    public async Task ReadTest()
    {
        IniLoader loader = new();

        using StreamReader sr = File.OpenText(Path.Combine("Assets", "Test2.ini"));
        var ini = await loader.ReadAsync(sr, default);

        Assert.IsNotNull(ini);
        Assert.IsTrue(ini.Default[0].IsEmptyKey);
        Assert.IsTrue(ini.Default[0].IsEmptyValue);
        Assert.IsTrue(!ini.Default[0].IsEmptySummary);
        Assert.IsTrue(ini.Default[0].Summary == "File Summary");

        Assert.IsTrue(ini.Sections[0].Name == "Section1");
        Assert.IsNotNull(ini.Sections[0].BeforeSummaries);
        Assert.IsTrue(string.Join("\n", ini.Sections[0].BeforeSummaries!.Select(i => i.Summary)) == "Before Summary");
        Assert.IsTrue(ini.Sections[0].Count() == 5);

        Assert.IsTrue(!ini.Sections[0][0].IsEmptyKey);
        Assert.IsTrue(ini.Sections[0][0].Key == "Key1");
        Assert.IsTrue(ini.Sections[0][0].IsEmptyValue);
        Assert.IsTrue(ini.Sections[0][0].IsEmptySummary);

    }
}
