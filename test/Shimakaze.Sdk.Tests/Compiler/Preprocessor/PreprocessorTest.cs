using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Compiler.Preprocessor;
using Shimakaze.Sdk.Compiler.Preprocessor.Commands;
using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Tests.Compiler.Preprocessor;

[TestClass]
public class PreprocessorTest
{

    [TestMethod]
    public async Task ConditionTestAsync()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddConditionCommands()
            .AddPreprocessor(builder => builder
                .AddDefine("TEST1")
                .AddDefines(new[] { "TEST2" })
            );

        await using var provider = services.BuildServiceProvider();
        var pp = provider.GetRequiredService<IPreprocessor>();
        string path = Path.Combine("Assets", "conditionTest.ini");
        using StreamReader reader = File.OpenText(path);
        await using MemoryStream ms = new();
        await using StreamWriter writer = new(ms);
        await pp.ExecuteAsync(reader, writer, path, CancellationToken.None);
        ms.Seek(0, SeekOrigin.Begin);
        using StreamReader sr = new(ms);
        var result = await sr.ReadToEndAsync();

        Regex regex = new("\\r?\\n");
        Assert.AreEqual(
            regex.Replace(
                """
                000
                111
                444
                666
                777
                999
                """.Trim(), "\n"),
            regex.Replace(result.Trim(), "\n")
        );

    }

    [TestMethod]
    public async Task DefineTestAsync()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddDefineCommands()
            .AddConditionCommands()
            .AddPreprocessor();

        await using var provider = services.BuildServiceProvider();
        var pp = provider.GetRequiredService<IPreprocessor>();
        string path = Path.Combine("Assets", "defineTest.ini");
        using StreamReader reader = File.OpenText(path);
        await using MemoryStream ms = new();
        await using StreamWriter writer = new(ms);
        await pp.ExecuteAsync(reader, writer, path, CancellationToken.None);
        ms.Seek(0, SeekOrigin.Begin);
        using StreamReader sr = new(ms);
        var result = await sr.ReadToEndAsync();

        Regex regex = new("\\r?\\n");
        Assert.AreEqual(
            regex.Replace(result.Trim(), "\n"),
            regex.Replace(
                """
                000
                222
                444
                """.Trim(), "\n"));

    }

    [TestMethod]
    public async Task AddRegionCommandsTestAsync()
    {
        var services = new ServiceCollection()
            .AddRegionCommands()
            .AddPreprocessor();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IEnumerable<IPreprocessorCommand> enumerable = serviceProvider.GetServices<IPreprocessorCommand>();

        Assert.AreEqual(2, enumerable.Count());
    }

    [TestMethod]
    public async Task AddCommandsTestAsync()
    {
        var services = new ServiceCollection()
            .AddCommand<RegionCommand>()
            .AddPreprocessor();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        Assert.IsInstanceOfType<RegionCommand>(serviceProvider.GetRequiredService<IPreprocessorCommand>());
    }

}
