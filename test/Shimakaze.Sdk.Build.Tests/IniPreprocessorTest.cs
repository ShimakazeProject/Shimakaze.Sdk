
using Microsoft.Build.Framework;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class IniPreprocessorTest
{
    private const string Assets = "Assets";
    private const string InputFile = "conditionTest.ini;defineTest.ini";
    private const string OutputPath = "Out";
    private const string OutputFile = "obj";
    private const string Defines = "DEFINED;TEST";
    private Mock<IBuildEngine>? buildEngine;
    private List<BuildErrorEventArgs>? errors;

    [TestInitialize]
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

    [TestMethod]
    public void Test()
    {
        IniPreprocessor task = new()
        {
            Files = string.Join(';', InputFile.Split(';').Select(i => Path.GetFullPath(Path.Combine(Assets, i)))),
            TargetDirectory = Path.Combine(Path.GetFullPath(OutputPath), OutputFile),
            BaseDirectory = Path.GetDirectoryName(Path.GetFullPath(OutputFile))!,
            Defines = Defines,
            BuildEngine = buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
        Assert.AreEqual(2, task.OutputFiles.Length);
    }
}
