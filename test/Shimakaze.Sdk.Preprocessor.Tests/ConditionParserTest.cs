namespace Shimakaze.Sdk.Preprocessor;

[TestClass]
public class ConditionParserTest
{
    private readonly IConditionParser _parser;
    public ConditionParserTest()
    {
        _parser = new ConditionParser(new PreprocessorVariables() { Defines = { "DEFINED" } });
    }

    [TestMethod]
    public void ANDTest()
    {
        Assert.AreEqual(_parser.Parse("TRUE && TRUE"), true && true, "T && T");
        Assert.AreEqual(_parser.Parse("FALSE && FALSE"), false && false, "F && F");

        Assert.AreEqual(_parser.Parse("TRUE && FALSE"), true && false, "T && F");
        Assert.AreEqual(_parser.Parse("FALSE && TRUE"), false && true, "F && T");

        Assert.AreEqual(_parser.Parse("TRUE && TRUE && TRUE"), true && true && true, "T && T && T");
        Assert.AreEqual(_parser.Parse("FALSE && FALSE && FALSE"), false && false && false, "F && F && F");

        Assert.AreEqual(_parser.Parse("TRUE && TRUE && FALSE"), true && true && false, "T && T && F");
        Assert.AreEqual(_parser.Parse("FALSE && FALSE && TRUE"), false && false && true, "F && F && T");

        Assert.AreEqual(_parser.Parse("TRUE && FALSE && FALSE"), true && false && false, "T && F && F");
        Assert.AreEqual(_parser.Parse("FALSE && TRUE && TRUE"), false && true && true, "F && T && T");

        Assert.AreEqual(_parser.Parse("TRUE && FALSE && TRUE"), true && false && true, "T && F && T");
        Assert.AreEqual(_parser.Parse("FALSE && TRUE && FALSE"), false && true && false, "F && T && F");

        Assert.AreEqual(_parser.Parse("DEFINED && DEFINED"), true && true, "T && T");
        Assert.AreEqual(_parser.Parse("UNDEFINED && UNDEFINED"), false && false, "F && F");

        Assert.AreEqual(_parser.Parse("DEFINED && UNDEFINED"), true && false, "T && F");
        Assert.AreEqual(_parser.Parse("UNDEFINED && DEFINED"), false && true, "F && T");

        Assert.AreEqual(_parser.Parse("DEFINED && DEFINED && DEFINED"), true && true && true, "T && T && T");
        Assert.AreEqual(_parser.Parse("UNDEFINED && UNDEFINED && UNDEFINED"), false && false && false, "F && F && F");

        Assert.AreEqual(_parser.Parse("DEFINED && DEFINED && UNDEFINED"), true && true && false, "T && T && F");
        Assert.AreEqual(_parser.Parse("UNDEFINED && UNDEFINED && DEFINED"), false && false && true, "F && F && T");

        Assert.AreEqual(_parser.Parse("DEFINED && UNDEFINED && UNDEFINED"), true && false && false, "T && F && F");
        Assert.AreEqual(_parser.Parse("UNDEFINED && DEFINED && DEFINED"), false && true && true, "F && T && T");

        Assert.AreEqual(_parser.Parse("DEFINED && UNDEFINED && DEFINED"), true && false && true, "T && F && T");
        Assert.AreEqual(_parser.Parse("UNDEFINED && DEFINED && UNDEFINED"), false && true && false, "F && T && F");
    }
    [TestMethod]
    public void ORTest()
    {
        Assert.AreEqual(_parser.Parse("TRUE || TRUE"), true || true, "T || T");
        Assert.AreEqual(_parser.Parse("FALSE || FALSE"), false || false, "F || F");

        Assert.AreEqual(_parser.Parse("TRUE || FALSE"), true || false, "T || F");
        Assert.AreEqual(_parser.Parse("FALSE || TRUE"), false || true, "F || T");

        Assert.AreEqual(_parser.Parse("TRUE || TRUE || TRUE"), true || true || true, "T || T || T");
        Assert.AreEqual(_parser.Parse("FALSE || FALSE || FALSE"), false || false || false, "F || F || F");

        Assert.AreEqual(_parser.Parse("TRUE || TRUE || FALSE"), true || true || false, "T || T || F");
        Assert.AreEqual(_parser.Parse("FALSE || FALSE || TRUE"), false || false || true, "F || F || T");

        Assert.AreEqual(_parser.Parse("TRUE || FALSE || FALSE"), true || false || false, "T || F || F");
        Assert.AreEqual(_parser.Parse("FALSE || TRUE || TRUE"), false || true || true, "F || T || T");

        Assert.AreEqual(_parser.Parse("TRUE || FALSE || TRUE"), true || false || true, "T || F || T");
        Assert.AreEqual(_parser.Parse("FALSE || TRUE || FALSE"), false || true || false, "F || T || F");

        Assert.AreEqual(_parser.Parse("DEFINED || DEFINED"), true || true, "T || T");
        Assert.AreEqual(_parser.Parse("UNDEFINED || UNDEFINED"), false || false, "F || F");

        Assert.AreEqual(_parser.Parse("DEFINED || UNDEFINED"), true || false, "T || F");
        Assert.AreEqual(_parser.Parse("UNDEFINED || DEFINED"), false || true, "F || T");

        Assert.AreEqual(_parser.Parse("DEFINED || DEFINED || DEFINED"), true || true || true, "T || T || T");
        Assert.AreEqual(_parser.Parse("UNDEFINED || UNDEFINED || UNDEFINED"), false || false || false, "F || F || F");

        Assert.AreEqual(_parser.Parse("DEFINED || DEFINED || UNDEFINED"), true || true || false, "T || T || F");
        Assert.AreEqual(_parser.Parse("UNDEFINED || UNDEFINED || DEFINED"), false || false || true, "F || F || T");

        Assert.AreEqual(_parser.Parse("DEFINED || UNDEFINED || UNDEFINED"), true || false || false, "T || F || F");
        Assert.AreEqual(_parser.Parse("UNDEFINED || DEFINED || DEFINED"), false || true || true, "F || T || T");

        Assert.AreEqual(_parser.Parse("DEFINED || UNDEFINED || DEFINED"), true || false || true, "T || F || T");
        Assert.AreEqual(_parser.Parse("UNDEFINED || DEFINED || UNDEFINED"), false || true || false, "F || T || F");
    }

    [TestMethod]
    public void NOTTest()
    {
        Assert.AreEqual(_parser.Parse("!TRUE"), !true, "!T");
        Assert.AreEqual(_parser.Parse("!FALSE"), !false, "!F");

        Assert.AreEqual(_parser.Parse("!DEFINED"), !true, "!T");
        Assert.AreEqual(_parser.Parse("!UNDEFINED"), !false, "!F");
    }

    [TestMethod]
    public void BooleanTest()
    {
        Assert.AreEqual(_parser.Parse("TRUE"), true, "T");
        Assert.AreEqual(_parser.Parse("FALSE"), false, "F");

        Assert.AreEqual(_parser.Parse("DEFINED"), true, "T");
        Assert.AreEqual(_parser.Parse("UNDEFINED"), false, "F");
    }

    [TestMethod]
    public void ParseTest()
    {
        Assert.AreEqual(_parser.Parse("!TRUE && FALSE || DEFINED || TRUE && TRUE"), !true && false || true || true && true);
    }

}