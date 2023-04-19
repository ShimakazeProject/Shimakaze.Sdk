using Microsoft.Build.Framework;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class CsfBuilderTests
{
    private const string Assets = "Assets";
    private const string InputXmlV1File = "ra2md.v1.csf.xml";
    private const string InputYamlV1File = "ra2md.v1.csf.yaml";
    private const string InputJsonV1File = "ra2md.v1.csf.json";
    private const string InputJsonV2File = "ra2md.v2.csf.json";
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
    public void JsonV1Test()
    {
        CsfBuilder task = new()
        {
            InputPath = Path.Combine(Assets, InputJsonV1File),
            Type = "JsonV1",
            OutputPath = Path.Combine(OutputPath, InputJsonV1File),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestMethod]
    public void JsonV2Test()
    {
        CsfBuilder task = new()
        {
            InputPath = Path.Combine(Assets, InputJsonV2File),
            Type = "JsonV2",
            OutputPath = Path.Combine(OutputPath, InputJsonV2File),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestMethod]
    public void YamlV1Test()
    {
        CsfBuilder task = new()
        {
            InputPath = Path.Combine(Assets, InputYamlV1File),
            Type = "YamlV1",
            OutputPath = Path.Combine(OutputPath, InputYamlV1File),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
    [TestMethod]
    public void XmlV1Test()
    {
        CsfBuilder task = new()
        {
            InputPath = Path.Combine(Assets, InputXmlV1File),
            Type = "XmlV1",
            OutputPath = Path.Combine(OutputPath, InputXmlV1File),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
    [TestMethod]
    public void UnknownTest()
    {
        CsfBuilder task = new()
        {
            InputPath = Path.Combine(Assets, InputXmlV1File),
            Type = "Unknown",
            OutputPath = Path.Combine(OutputPath, "Unknown"),
            BuildEngine = _buildEngine?.Object,
        };
        Assert.ThrowsException<NotSupportedException>(() => task.Execute());
    }
}