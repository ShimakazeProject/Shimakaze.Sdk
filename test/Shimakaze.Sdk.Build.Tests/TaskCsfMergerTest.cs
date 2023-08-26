using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class TaskCsfMergerTest
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputPath = "Out";
    private Mock<IBuildEngine>? _buildEngine;
    private List<BuildErrorEventArgs>? _errors;

    [TestMethod]
    public void MergeTest()
    {
        TaskItem item = new(Path.Combine(Assets, InputFile));

        TaskCsfMerger task = new()
        {
            SourceFiles = new[] { item },
            DestinationFile = Path.Combine(OutputPath, InputFile),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestInitialize]
    public void Startup()
    {
        _buildEngine = new Mock<IBuildEngine>();
        _errors = new List<BuildErrorEventArgs>();
        _buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => _errors.Add(e));

        Directory.CreateDirectory(OutputPath);
    }
}