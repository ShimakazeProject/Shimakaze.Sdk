using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class TaskMixGeneratorTest
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputFile = "MixPackerTest.mix";
    private const string OutputPath = "Out";
    private Mock<IBuildEngine>? _buildEngine;
    private List<BuildErrorEventArgs>? _errors;

    [TestInitialize]
    public void Startup()
    {
        _buildEngine = new Mock<IBuildEngine>();
        _errors = [];
        _buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => _errors.Add(e));

        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void Test()
    {
        TaskItem item = new(Path.Combine(Assets, InputFile));

        TaskMixGenerator task = new()
        {
            SourceFiles = new[] { item },
            DestinationFile = Path.Combine(OutputPath, OutputFile),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}