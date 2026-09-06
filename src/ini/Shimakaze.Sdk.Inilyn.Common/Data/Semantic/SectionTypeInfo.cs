namespace Shimakaze.Sdk.Inilyn.Data.Semantic;

using Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 节的类型分配（多对多）：一个节可以有多个类型，一个类型可以分配给多个节。
/// </summary>
public sealed class SectionTypeInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SectionId { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public SectionNode? Section { get; set; }
}