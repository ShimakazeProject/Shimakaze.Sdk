using System.Text.Json;

namespace Shimakaze.Sdk.Ini.Ares.Tests;

[TestClass()]
public class AresIniDocumentBinderTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    [TestMethod()]
    public void BindTest()
    {
        const string ini = """
            ; 节前注释
            [Section1] ; 行内注释
            ; 注释
            Key1=Value1 ; 注释
            Key2=Value2
            Key3=Value3
            Key4

            ; 节前注释
            [Section2 ABC] : [Section1] ; 行内注释
            ; 注释
            Key1 ABC = Value1 ABC ; 注释
             Key3=ValueNot3
            Key4=Value4

            [Section3]
            += 1
            += 2
            += 3
            += 4
            += 5
            += 6
            += 7
            += 8
            += 9
            += 10
            """;

        using StringReader sr = new(ini);
        using AresIniTokenReader reader = new(sr);
        using AresIniDocumentBinder binder = new(reader);
        AresIniDocument doc = binder.Bind();

        Console.WriteLine(JsonSerializer.Serialize(doc, _options));

        Assert.IsNotNull(doc);
        Assert.AreEqual("Value1", doc["Section1", "Key1"]);
        Assert.AreEqual("Value2", doc["Section1", "Key2"]);
        Assert.AreEqual("Value3", doc["Section1", "Key3"]);
        Assert.IsTrue(doc["Section1"].ContainsKey("Key4"));

        Assert.AreEqual("Section1", doc["Section2 ABC"].BaseName);
        Assert.AreEqual("Value1 ABC", doc["Section2 ABC", "Key1 ABC"]);
        Assert.AreEqual("Value2", doc["Section2 ABC", "Key2"]);
        Assert.AreEqual("ValueNot3", doc["Section2 ABC", "Key3"]);
        Assert.AreEqual("Value4", doc["Section2 ABC", "Key4"]);

        Assert.AreEqual(10, doc["Section3"].Count);
    }

}