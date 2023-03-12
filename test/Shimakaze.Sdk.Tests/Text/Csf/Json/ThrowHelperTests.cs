using System.Text.Json;

using Shimakaze.Sdk.Text.Csf.Json;

namespace Shimakaze.Sdk.Tests.Text.Csf.Json;

[TestClass]
public class ThrowHelperTests
{
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
}