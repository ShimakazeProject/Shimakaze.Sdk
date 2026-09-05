using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// 
/// </summary>
[YamlStaticContext]
//[YamlSerializable(typeof(CsfData))]
//[YamlSerializable(typeof(CsfLabel))]
//[YamlSerializable(typeof(CsfValue))]
[YamlSerializable(typeof(CsfMetadata))]
[YamlSerializable(typeof(CsfLanguage))]
public sealed partial class CsfYamlV1StaticContext : StaticContext
{
}
