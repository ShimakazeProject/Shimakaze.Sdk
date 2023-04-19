
using Microsoft.Build.Framework;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class MixPackerTest
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputPath = "Out";
    private const string OutputFile = "MixPackerTest.mix";
    private Mock<IBuildEngine>? _buildEngine;
    private List<BuildErrorEventArgs>? _errors;

    [TestInitialize]
    public void Startup()
    {
        _buildEngine = new Mock<IBuildEngine>();
        _errors = new List<BuildErrorEventArgs>();
        _buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => _errors.Add(e));

        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void Test()
    {
        MixPacker task = new()
        {
            Files = Path.Combine(Assets, InputFile),
            TargetFile = Path.Combine(OutputPath, OutputFile),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}
