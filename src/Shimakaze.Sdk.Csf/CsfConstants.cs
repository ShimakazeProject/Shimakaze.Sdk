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
    /// Value string Encode/decode<br/>
    /// The Unicode encoded content in CSF documents are all bitwise isochronous<br/>
    /// This method is implemented using a for loop.
    /// </summary>
    /// <param name="data">data.</param>
    /// <param name="length">length.</param>
    /// <param name="start">start index.</param>
    public static unsafe void CodingValue(byte* data, int length, int start = 0)
    {
        for (int i = 0; i < length; i++)
        {
            data[start + i] = (byte)~data[start + i];
        }
    }

    /// <summary>
    /// Value string Encode/decode<br/>
    /// The Unicode encoded content in CSF documents are all bitwise isochronous<br/>
    /// This method is implemented using a for loop.
    /// </summary>
    /// <param name="data">data.</param>
    /// <returns>result.</returns>
    public static byte[] CodingValue(byte[] data)
    {
        unsafe
        {
            fixed (byte* ptr = data)
            {
                CodingValue(ptr, data.Length);
                return data;
            }
        }
    }
}