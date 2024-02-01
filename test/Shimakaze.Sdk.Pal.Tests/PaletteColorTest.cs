namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class PaletteColorTest
{
    private PaletteColor _color;

    [TestMethod]
    public void GetColorTest()
    {
        Assert.AreEqual(248, _color.Red);
        Assert.AreEqual(252, _color.Green);
        Assert.AreEqual(248, _color.Blue);
        var pixel = _color.AsRGB565();
        Assert.AreEqual(65535, pixel);
        Assert.AreEqual("#F8FCF8", _color.ToString());
    }

    [TestInitialize]
    public void Init()
    {
        _color = new PaletteColor(63, 63, 63).AsDisplaydColor();
    }
}