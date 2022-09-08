namespace Shimakaze.Sdk.Loader.Ini.Test;

[TestClass]
public class IniLoaderTest
{
    [TestMethod]
    public async Task ReadTest()
    {
        IniLoader loader = new();

        using StreamReader sr = File.OpenText(Path.Combine("Assets", "Test1.ini"));
        var ini = await loader.ReadAsync(sr, default);

        Assert.IsNotNull(ini);
        Assert.IsTrue(ini.Default[0].IsEmptyKey);
        Assert.IsTrue(ini.Default[0].IsEmptyValue);
        Assert.IsTrue(!ini.Default[0].IsEmptySummary);
        Assert.IsTrue(ini.Default[0].Summary == "File Summary");

        Assert.IsTrue(ini.Sections[0].Name == "Section1");
        Assert.IsNotNull(ini.Sections[0].BeforeSummaries);
        Assert.IsTrue(string.Join("\n", ini.Sections[0].BeforeSummaries!.Select(i => i.Summary)) == "Before Summary");
        Assert.IsTrue(ini.Sections[0].Count() == 3);

        Assert.IsTrue(!ini.Sections[0][0].IsEmptyKey);
        Assert.IsTrue(ini.Sections[0][0].Key == "Key1");
        Assert.IsTrue(!ini.Sections[0][0].IsEmptyValue);
        Assert.IsTrue(ini.Sections[0][0].Value!.ToString() == "Value1");
        Assert.IsTrue(ini.Sections[0][0].IsEmptySummary);

        Assert.IsTrue(!ini.Sections[0][1].IsEmptyKey);
        Assert.IsTrue(ini.Sections[0][1].Key == "Key2");
        Assert.IsTrue(!ini.Sections[0][1].IsEmptyValue);
        Assert.IsTrue(ini.Sections[0][1].Value!.ToString() == "Value2");
        Assert.IsTrue(!ini.Sections[0][1].IsEmptySummary);
        Assert.IsTrue(ini.Sections[0][1].Summary == "Inline Sumamry");

        Assert.IsTrue(ini.Sections[0][2].IsEmptyKey);
        Assert.IsTrue(ini.Sections[0][2].IsEmptyValue);
        Assert.IsTrue(!ini.Sections[0][2].IsEmptySummary);
        Assert.IsTrue(ini.Sections[0][2].Summary == "After Summary");

        Assert.IsTrue(ini.Sections[1].Name == "Section2");
        Assert.IsNotNull(ini.Sections[1].BeforeSummaries);
        Assert.IsTrue(string.Join("\n", ini.Sections[1].BeforeSummaries!.Select(i => i.Summary)) == "Before Summary");
        Assert.IsTrue(ini.Sections[1].Count() == 3);

        Assert.IsTrue(!ini.Sections[1][0].IsEmptyKey);
        Assert.IsTrue(ini.Sections[1][0].Key == "Key1");
        Assert.IsTrue(!ini.Sections[1][0].IsEmptyValue);
        Assert.IsTrue(ini.Sections[1][0].Value!.ToString() == "Value1");
        Assert.IsTrue(ini.Sections[1][0].IsEmptySummary);

        Assert.IsTrue(!ini.Sections[1][1].IsEmptyKey);
        Assert.IsTrue(ini.Sections[1][1].Key == "Key2");
        Assert.IsTrue(!ini.Sections[1][1].IsEmptyValue);
        Assert.IsTrue(ini.Sections[1][1].Value!.ToString() == "Value2");
        Assert.IsTrue(!ini.Sections[1][1].IsEmptySummary);
        Assert.IsTrue(ini.Sections[1][1].Summary == "Inline Sumamry");

        Assert.IsTrue(ini.Sections[1][2].IsEmptyKey);
        Assert.IsTrue(ini.Sections[1][2].IsEmptyValue);
        Assert.IsTrue(!ini.Sections[1][2].IsEmptySummary);
        Assert.IsTrue(ini.Sections[1][2].Summary == "After Summary");
    }
}
