namespace Shimakaze.Sdk.Inilyn.Data;

/// <summary>
/// 文件信息
/// </summary>
public sealed class IniDocument
{
    public Guid Id { get; set; }
    /// <summary>
    /// 区分大小写的文件路径 索引 唯一
    /// </summary>
    public string Path { get; set; } = null!;
    /// <summary>
    /// 文件所属的分类
    /// </summary>
    public IniCategory Category { get; set; } = null!;
    /// <summary>
    /// 文件内容的哈希值 用于判断文件是否被修改
    /// </summary>
    public string Checksum { get; set; } = null!;
}