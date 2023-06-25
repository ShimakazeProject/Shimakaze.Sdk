namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class ColorTest
{
    private Color _color;

    [TestInitialize]
    public void Init()
    {
        _color = new(63, 63, 63);

    }

    [TestMethod]
    public void GetColorTest()
    {
        Assert.AreEqual(248, _color.GetR5());
        Assert.AreEqual(252, _color.GetG6());
        Assert.AreEqual(248, _color.GetB5());
        Assert.AreEqual(65535, _color.GetRGB565());
        Assert.AreEqual("#3F3F3F", _color.ToString());
    }
}