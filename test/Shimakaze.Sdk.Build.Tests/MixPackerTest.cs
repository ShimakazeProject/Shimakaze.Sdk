
using Microsoft.Build.Framework;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass()]
public class MixPackerTest
{
    private const string Assets = "Assets";
    private const string InputFile = "TestFile.ini";
    private const string OutputPath = "Out";
    private const string OutputFile = "MixPackerTest.mix";
    private Mock<IBuildEngine>? buildEngine;
    private List<BuildErrorEventArgs>? errors;

    [TestInitialize()]
    public void Startup()
    {
        buildEngine = new Mock<IBuildEngine>();
        errors = new List<BuildErrorEventArgs>();
        buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e));

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }

    [TestMethod()]
    public void Test()
    {
        MixPacker task = new()
        {
            Files = Path.Combine(Assets, InputFile),
            TargetFile = Path.Combine(OutputPath, OutputFile),
            BuildEngine = buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}
