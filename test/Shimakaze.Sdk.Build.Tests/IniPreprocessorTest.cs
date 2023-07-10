using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class IniPreprocessorTest
{
    private const string Assets = "Assets";
    private const string Defines = "DEFINED;TEST";
    private const string InputFile = "conditionTest.ini;defineTest.ini";
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
    public void Test()
    {
        IniPreprocessor task = new()
        {
            SourceFiles = InputFile.Split(';').Select(i =>
            {
                var path = Path.GetFullPath(Path.Combine(Assets, i));
                TaskItem item = new(Path.GetFullPath(Path.Combine(Assets, i)));
                item.SetMetadata("Destination", Path.GetFullPath(Path.Combine(OutputPath, i)));
                return item;
            }).ToArray(),
            Defines = Defines,
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}