using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Shimakaze.Sdk.Build;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private Mock<IBuildEngine>? buildEngine;
    private List<BuildErrorEventArgs>? errors;

    [TestInitialize]
    public void Startup()
    {
        buildEngine = new Mock<IBuildEngine>();
        errors = new List<BuildErrorEventArgs>();
        buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e));

        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void JsonV1Test()
    {
        CsfBuilder task = new()
        {
            Files = Path.Combine(Assets, InputJsonV1File),
            Type = "JsonV1",
            TargetDirectory = Path.Combine(OutputPath),
            BuildEngine = buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestMethod]
    public void JsonV2Test()
    {
        CsfBuilder task = new()
        {
            Files = Path.Combine(Assets, InputJsonV2File),
            Type = "JsonV2",
            TargetDirectory = Path.Combine(OutputPath),
            BuildEngine = buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }

    [TestMethod]
    public void YamlV1Test()
    {
        CsfBuilder task = new()
        {
            Files = Path.Combine(Assets, InputYamlV1File),
            Type = "YamlV1",
            TargetDirectory = Path.Combine(OutputPath),
            BuildEngine = buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
    [TestMethod]
    public void XmlV1Test()
    {
        CsfBuilder task = new()
        {
            Files = Path.Combine(Assets, InputXmlV1File),
            Type = "XmlV1",
            TargetDirectory = Path.Combine(OutputPath),
            BuildEngine = buildEngine?.Object,
        };
        Assert.IsTrue(task.Execute());
    }
}