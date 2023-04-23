using Microsoft.Build.Framework;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class IniMergerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "normal.Ini";
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
        IniMerger task = new()
        {
            IniFiles = Path.Combine(Assets, InputFile),
            OutputPath = Path.Combine(OutputPath, InputFile),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}