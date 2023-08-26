using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Moq;

namespace Shimakaze.Sdk.Build.Tests;

[TestClass]
public class TaskCsfGeneratorTest
{
    private const string Assets = "Assets";
    private const string InputJsonV1File = "ra2md.v1.csf.json";
    private const string InputJsonV2File = "ra2md.v2.csf.json";
    private const string InputXmlV1File = "ra2md.v1.csf.xml";
    private const string InputYamlV1File = "ra2md.v1.csf.yaml";
    private const string OutputPath = "Out";
    private Mock<IBuildEngine>? _buildEngine;
    private List<BuildErrorEventArgs>? _errors;

    [TestMethod]
    public void JsonV1Test()
    {
        TaskItem item = new(Path.Combine(Assets, InputJsonV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Destination, Path.Combine(OutputPath, InputJsonV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Type, "JsonV1");

        TaskCsfGenerator task = new()
        {
            SourceFiles = new[] { item },
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestMethod]
    public void JsonV2Test()
    {
        TaskItem item = new(Path.Combine(Assets, InputJsonV2File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Destination, Path.Combine(OutputPath, InputJsonV2File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Type, "JsonV2");

        TaskCsfGenerator task = new()
        {
            SourceFiles = new[] { item },
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

    [TestMethod]
    public void UnknownTest()
    {
        TaskItem item = new(Path.Combine(Assets, InputXmlV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Destination, Path.Combine(OutputPath, "Unknown"));
        item.SetMetadata(TaskCsfGenerator.Metadata_Type, "Unknown");

        TaskCsfGenerator task = new()
        {
            SourceFiles = new[] { item },
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsFalse(task.Execute());
    }

    [TestMethod]
    public void XmlV1Test()
    {
        TaskItem item = new(Path.Combine(Assets, InputXmlV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Destination, Path.Combine(OutputPath, InputXmlV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Type, "XmlV1");

        TaskCsfGenerator task = new()
        {
            SourceFiles = new[] { item },
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestMethod]
    public void YamlV1Test()
    {
        TaskItem item = new(Path.Combine(Assets, InputYamlV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Destination, Path.Combine(OutputPath, InputYamlV1File));
        item.SetMetadata(TaskCsfGenerator.Metadata_Type, "YamlV1");

        TaskCsfGenerator task = new()
        {
            SourceFiles = new[] { item },
            BuildEngine = _buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}