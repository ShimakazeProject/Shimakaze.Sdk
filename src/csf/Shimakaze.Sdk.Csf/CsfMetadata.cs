namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CSF 文件元数据
/// </summary>
public record struct CsfMetadata()
{
    /// <summary>
    /// CSF 文件的魔术头，它永远是“CSF ”，即<br />
    /// <code>
    ///      00 01 02 03 | ASCII
    /// 0x00 43 53 46 20 | FSC 
    /// </code>
    /// </summary>
    public int Identifier { get; set; } = CsfConstants.CsfFlagRaw;

    /// <summary>
    /// CSF 文件的版本。通常情况下，它应该始终为 <c>3</c>
    /// </summary>
    public int Version { get; set; } = 3;

    /// <summary>
    /// 标签数量。CSF 文件中包含的所有的标签的总数
    /// </summary>
    public int LabelCount { get; set; }

    /// <summary>
    /// 字符串数量。一般情况下，它应该与标签数相同。
    /// </summary>
    /// <remarks>
    /// 字符串数量是将所有标签中的字符串值计数得到的
    /// </remarks>
    public int StringCount { get; set; }

    /// <summary>
    /// 保留。作用未知。
    /// </summary>
    public int Unknown { get; set; }

    /// <summary>
    /// 使用的语言。
    /// </summary>
    public CsfLanguage Language { get; set; } = CsfLanguage.ENUS;

    /// <summary>
    /// Modifies the metadata using the provided delegate.
    /// </summary>
    /// <param name="metadata"></param>
    public delegate void ModifyDelegate(ref CsfMetadata metadata);

    /// <summary>
    /// Modifies the metadata using the provided delegate.
    /// </summary>
    /// <param name="modify"></param>
    public void Modify(ModifyDelegate modify) => modify(ref this);
}
