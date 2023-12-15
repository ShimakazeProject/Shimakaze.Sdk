using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Tests;

[TestClass]
public sealed class Rgb24Test
{
    private Rgb24 _color;

    [TestMethod]
    public void GetColorTest()
    {
        Assert.AreEqual(248, _color.Red);
        Assert.AreEqual(252, _color.Green);
        Assert.AreEqual(248, _color.Blue);
        _color.ToRgb565(out Rgb565 pixel);
        Assert.AreEqual(65535, pixel.Value);
        Assert.AreEqual("#F8FCF8", _color.ToString());
    }

    [TestInitialize]
    public void Init()
    {
        _color = new Rgb24(63, 63, 63).ToGameColor();
    }
}