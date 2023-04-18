namespace Shimakaze.Sdk.Preprocessor.Commands;

[TestClass]
public class CommandTests
{
    [TestMethod]
    public async Task DefineTestAsync()
    {
        DefineCommand cmd = new(new PreprocessorVariables());
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }

    [TestMethod]
    public async Task ElifTestAsync()
    {
        PreprocessorVariables variables = new();
        ElifCommand cmd = new(variables, new ConditionParser(variables));
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }

    [TestMethod]
    public async Task EndregionTestAsync()
    {
        EndregionCommand cmd = new(new PreprocessorVariables());
        await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task IfTestAsync()
    {
        PreprocessorVariables variables = new();
        IfCommand cmd = new(variables, new ConditionParser(variables));
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }

    [TestMethod]
    public async Task RegionTestAsync()
    {
        RegionCommand cmd = new(new PreprocessorVariables());
        await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task UndefTestAsync()
    {
        UndefCommand cmd = new(new PreprocessorVariables());
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }
}
