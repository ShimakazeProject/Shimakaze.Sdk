using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json.Tests;

[TestClass]
public class ThrowHelperTests
{
    [TestMethod]
    public void ThrowNotSupportTokenTest()
    {
        Assert.ThrowsException<NotSupportedException>(() => JsonTokenType.Null.ThrowNotSupportToken<int>());
    }

    [TestMethod]
    public void ThrowNotSupportValueTest()
    {
        Assert.ThrowsException<NotSupportedException>(() => JsonTokenType.Null.ThrowNotSupportValue<JsonTokenType, int>());
    }

    [TestMethod]
    public void ThrowWhenFalseTest()
    {
        Assert.ThrowsException<JsonException>(() => false.ThrowWhenFalse());
    }

    [TestMethod]
    public void ThrowWhenNotTokenTest()
    {
        Assert.ThrowsException<NotSupportedException>(() => JsonTokenType.Null.ThrowWhenNotToken(JsonTokenType.True));
    }

    [TestMethod]
    public void ThrowWhenNullTest()
    {
        Assert.ThrowsException<ArgumentNullException>(() => ((object?)null).ThrowWhenNull());
    }
}