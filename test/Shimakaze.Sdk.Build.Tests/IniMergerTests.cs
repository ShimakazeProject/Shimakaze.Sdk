using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class IniMergerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "normal.ini";
    private const string OutputPath = "Out";
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
    public void MergeTest()
    {
        TaskItem item = new(Path.Combine(Assets, InputFile));
        item.SetMetadata(IniMerger.Metadata_Type, "Rule");
        TaskItem output = new("Rule");
        output.SetMetadata(IniMerger.Metadata_Destination, Path.Combine(OutputPath, InputFile));
        IniMerger task = new()
        {
            SourceFiles = new[] { item },
            DestinationFiles = new[] { output },
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}