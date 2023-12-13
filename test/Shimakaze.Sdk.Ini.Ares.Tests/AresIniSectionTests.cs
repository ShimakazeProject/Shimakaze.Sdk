namespace Shimakaze.Sdk.Ini.Ares.Tests;

[TestClass()]
public class AresIniSectionTests
{
    private AresIniSection _section = null!;

    [TestInitialize()]
    public void Initialize()
    {
        _section = new("Section", "BaseSection")
        {
            ["Key1"] = "Value1"
        };
    }

    [TestMethod()]
    public void IniSectionTest()
    {
        Assert.IsNotNull(_section);
        Assert.AreEqual("BaseSection", _section.BaseName);
        _section = new("Section", default, new()
        {
            ["Key1"] = "Value1"
        });
    }

    [TestMethod()]
    public void AddTest()
    {
        _section.Add("Key2", "Value2");
        Assert.AreEqual("Value2", _section["Key2"]);
    }

    [TestMethod()]
    public void ContainsKeyTest()
    {
        Assert.AreEqual(true, _section.ContainsKey("Key1"));
        Assert.IsFalse(_section.ContainsKey("Key2"));

    }

    [TestMethod()]
    public void RemoveTest()
    {
        _section.Remove("Key1");
        Assert.IsFalse(_section.ContainsKey("Key1"));
    }

    [TestMethod()]
    public void TryGetValueTest()
    {
        Assert.IsTrue(_section.TryGetValue("Key1", out var value));
        Assert.AreEqual("Value1", value);
        Assert.IsFalse(_section.TryGetValue("Key2", out value));
        Assert.IsNull(value);
    }

    [TestMethod()]
    public void ClearTest()
    {
        _section.Clear();
        Assert.AreEqual(0, _section.Count);
    }

    [TestMethod()]
    public void GetEnumeratorTest()
    {
        using var enumerator = _section.GetEnumerator();
        Assert.IsNotNull(enumerator);
    }
}