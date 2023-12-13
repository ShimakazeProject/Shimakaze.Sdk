using System.Text.Json;

namespace Shimakaze.Sdk.Ini.Tests;

[TestClass()]
public class IniDocumentBinderTests
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
            Key3=
            Key4

            ; 节前注释
            [Section2 ABC] ; 行内注释
            ; 注释
            Key1 ABC = Value1 ABC ; 注释
             Key2 = Value2
            Key3=
            Key4
            """;

        using StringReader sr = new(ini);
        using IniTokenReader reader = new(sr);
        using IniDocumentBinder binder = new(reader);
        IniDocument doc = binder.Bind();

        Console.WriteLine(JsonSerializer.Serialize(doc, _options));

        Assert.IsNotNull(doc);
        Assert.AreEqual("Value1", doc["Section1", "Key1"]);
        Assert.AreEqual("Value2", doc["Section1", "Key2"]);
        Assert.IsTrue(doc["Section1"].ContainsKey("Key3"));
        Assert.IsTrue(doc["Section1"].ContainsKey("Key4"));

        Assert.AreEqual("Value1 ABC", doc["Section2 ABC", "Key1 ABC"]);
        Assert.AreEqual("Value2", doc["Section2 ABC", "Key2"]);
        Assert.IsTrue(doc["Section2 ABC"].ContainsKey("Key3"));
        Assert.IsTrue(doc["Section2 ABC"].ContainsKey("Key4"));
    }

}