using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shimakaze.Sdk.CompilerCollection.Preprocessor.Commands;
using Shimakaze.Sdk.CompilerCollection.Preprocessor.Kernel;

namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Tests;

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
                .AddDefine("TEST")
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
            regex.Replace(result.Trim(), "\n"),
            regex.Replace(
                """
                000
                111
                444
                """.Trim(), "\n"));

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
}
