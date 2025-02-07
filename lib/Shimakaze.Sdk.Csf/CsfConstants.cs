namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CsfConstants.
/// </summary>
public static class CsfConstants
{
    /// <summary>
    /// ' FSC'.
    /// </summary>
    public const int CsfFlagRaw = 0x43_53_46_20;

    /// <summary>
    /// ' LBL'.
    /// </summary>
    public const int LblFlagRaw = 0x4C_42_4C_20;

    /// <summary>
    /// ' RTS'.
    /// </summary>
    public const int StrFlagRaw = 0x53_54_52_20;

    /// <summary>
    /// 'WRTS'.
    /// </summary>
    public const int StrwFlgRaw = 0x53_54_52_57;

    /// <summary>
    /// 编/解码 CSF 值
    /// </summary>
    /// <param name="data"></param>
    public static Span<byte> CodingValue(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)~data[i];

        return data;
    }
}
