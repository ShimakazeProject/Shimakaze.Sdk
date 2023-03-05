using Shimakaze.Sdk.Compiler.Preprocessor.Commands;
using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Tests.Compiler.Preprocessor.Commands;

[TestClass()]
public class CommandTests
{
    [TestMethod()]
    public async Task DefineTestAsync()
    {
        DefineCommand cmd = new(IPreprocessorVariables.Create());
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }

    [TestMethod()]
    public async Task ElifTestAsync()
    {
        ElifCommand cmd = new(IPreprocessorVariables.Create(), IConditionParser.Create());
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }

    [TestMethod()]
    public async Task EndregionTestAsync()
    {
        EndregionCommand cmd = new(IPreprocessorVariables.Create());
        await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
    }

    [TestMethod()]
    public async Task IfTestAsync()
    {
        IfCommand cmd = new(IPreprocessorVariables.Create(), IConditionParser.Create());
        try
        {
            await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Invalid arguments", e.Message);
        }
    }
    
    [TestMethod()]
    public async Task RegionTestAsync()
    {
        RegionCommand cmd = new(IPreprocessorVariables.Create());
        await cmd.ExecuteAsync(Array.Empty<string>(), default).ConfigureAwait(false);
    }
    
    [TestMethod()]
    public async Task UndefTestAsync()
    {
        UndefCommand cmd = new(IPreprocessorVariables.Create());
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
