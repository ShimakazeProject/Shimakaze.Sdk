namespace Shimakaze.Sdk.Ini.Tests;

[TestClass()]
public class IniSectionTests
{
    private IniSection _section = null!;

    [TestInitialize()]
    public void Initialize()
    {
        _section = new("Section")
        {
            ["Key1"] = "Value1"
        };
    }

    [TestMethod()]
    public void IniSectionTest()
    {
        Assert.IsNotNull(_section);
        _section = new("Section", new()
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
        Assert.IsTrue(_section.ContainsKey("Key1"));
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