namespace Shimakaze.Sdk.Ini.Tests;

[TestClass()]
public class IniDocumentTests
{
    private IniDocument _ini = null!;

    [TestInitialize()]
    public void Initialize()
    {
        _ini = [
            new IniSection("Section")
            {
                ["Key1"] = "Value1"
            }
        ];
    }

    [TestMethod()]
    public void IniDocumentTest()
    {
        Assert.IsNotNull(new IniDocument());
        Assert.IsNotNull(_ini);
    }

    [TestMethod()]
    public void AddTest()
    {
        const string section = "Section1";
        _ini.Add(new(section));
        Assert.IsNotNull(_ini[section]);
    }

    [TestMethod()]
    public void ClearTest()
    {
        _ini.Clear();
        Assert.AreEqual(0, _ini.Count);
    }

    [TestMethod()]
    public void ContainsSectionTest()
    {
        Assert.IsTrue(_ini.ContainsSection("Section"));
        Assert.IsFalse(_ini.ContainsSection("Section1"));
    }


    [TestMethod()]
    public void RemoveTest()
    {
        _ini.Remove("Section");
        Assert.IsFalse(_ini.ContainsSection("Section"));
    }

    [TestMethod()]
    public void TryGetSectionTest()
    {
        Assert.IsTrue(_ini.TryGetSection("Section", out var section));
        Assert.IsNotNull(section);
        Assert.IsFalse(_ini.TryGetSection("Section1", out section));
        Assert.IsNull(section);
    }

    [TestMethod()]
    public void GetEnumeratorTest()
    {
        using var enumerator = _ini.GetEnumerator();
        Assert.IsNotNull(enumerator);
    }
}