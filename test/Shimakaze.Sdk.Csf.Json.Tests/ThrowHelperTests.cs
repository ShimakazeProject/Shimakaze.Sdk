using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

[TestClass]
public class ThrowHelperTests
{
    [TestMethod]
    public void ThrowNotSupportTokenTest()
    {
        try
        {
            JsonTokenType.Null.ThrowNotSupportToken<int>();
        }
        catch (JsonException)
        {
        }
    }

    [TestMethod]
    public void ThrowNotSupportValueTest()
    {
        try
        {
            JsonTokenType.Null.ThrowNotSupportValue<JsonTokenType, int>();
        }
        catch (JsonException)
        {
        }
    }

    [TestMethod]
    public void ThrowWhenFalseTest()
    {
        try
        {
            false.ThrowWhenFalse();
        }
        catch (JsonException)
        {
        }
    }

    [TestMethod]
    public void ThrowWhenNotTokenTest()
    {
        try
        {
            JsonTokenType.Null.ThrowWhenNotToken(JsonTokenType.True);
        }
        catch (JsonException)
        {
        }
    }

    [TestMethod]
    public void ThrowWhenNullTest()
    {
        try
        {
            ((object?)null).ThrowWhenNull();
        }
        catch (ArgumentNullException)
        {
        }
    }
}