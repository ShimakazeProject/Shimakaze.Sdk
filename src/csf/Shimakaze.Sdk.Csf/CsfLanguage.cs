namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CSF 规范支持的语言
/// </summary>
/// <remarks>
/// 如有必要，可以使用强制转换使用未被列出的值。
/// </remarks>
/// <param name="Value"></param>
public record struct CsfLanguage(int Value)
{
    /// <summary>
    /// (Ares) 语言中性
    /// </summary>
    /// <remarks>
    /// 无论主字符串表 ra2md.csf 是什么语言，将始终加载非特定语言的文件。
    /// </remarks>
    public const int AresNeutral = -1;
    /// <summary>
    /// 英语（美国）
    /// </summary>
    public const int ENUS = 0;
    /// <summary>
    /// 英语（英国）
    /// </summary>
    public const int ENUK = 1;
    /// <summary>
    /// 德语
    /// </summary>
    public const int DE = 2;
    /// <summary>
    /// 法语
    /// </summary>
    public const int FR = 3;
    /// <summary>
    /// 西班牙语
    /// </summary>
    public const int ES = 4;
    /// <summary>
    /// 意大利语
    /// </summary>
    public const int IT = 5;
    /// <summary>
    /// 日语
    /// </summary>
    public const int JA = 6;
    /// <summary>
    /// 无意义
    /// </summary>
    public const int Jabberwockie = 7;
    /// <summary>
    /// 韩语
    /// </summary>
    public const int KR = 8;
    /// <summary>
    /// 中文（繁体）
    /// </summary>
    public const int ZH = 9;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="language"></param>
    public static implicit operator int(in CsfLanguage language) => language.Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator CsfLanguage(in int value) => new(value);

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public override readonly string ToString() => Value switch
    {
        0 => "en_US",
        1 => "en_UK",
        2 => "de",
        3 => "fr",
        4 => "es",
        5 => "it",
        6 => "jp",
        7 => "Jabberwockie",
        8 => "kr",
        9 => "zh",
        _ => $"{Value}",
    };

    /// <summary>
    /// 从字符串中解析
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static CsfLanguage Parse(string? value) => value switch
    {
        "en_US" => 0,
        "en_UK" => 1,
        "de" => 2,
        "fr" => 3,
        "es" => 4,
        "it" => 5,
        "jp" => 6,
        "Jabberwockie" => 7,
        "kr" => 8,
        "zh" => 9,
        _ => 0
    };
}
