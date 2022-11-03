namespace Shimakaze.Sdk.Loader.Ini.Test;

[TestClass]
public class IncompleteIniReaderTest
{
    
    [TestMethod]
    public void ReadTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine("Assets", "Test2.ini"));
        IniReader reader = new(sr);
        
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Comment);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Comment);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.SectionHeader);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Comment);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Key);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Key);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Value);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Key);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Value);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Comment);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Key);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Value);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Comment);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Value);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Token is IniToken.Comment);
    }
}