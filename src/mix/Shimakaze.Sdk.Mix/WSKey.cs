using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Numerics;


namespace Shimakaze.Sdk.Mix;

internal static class WSKey
{
    private static readonly Lazy<(BigInteger Modulus, BigInteger Exponent)> Publickey = new(InitPubKey);

    private static (BigInteger Modulus, BigInteger Exponent) InitPubKey()
    {
        var base64 = "AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V"u8;
        var len = Base64.GetMaxDecodedFromUtf8Length(base64.Length);
        Span<byte> data = stackalloc byte[len];
        var status = Base64.DecodeFromUtf8(base64, data, out _, out len);
        Debug.Assert(status is OperationStatus.Done);
        data = data[..len];

        int index = 0;

        if (data[index++] != 0x02)
            throw new FormatException("Expected INTEGER (modulus)");

        int length = ReadAsn1Length(data, ref index);
        BigInteger modulus = new(data.Slice(index, length), isBigEndian: true, isUnsigned: true);

        return (modulus, 0x10001u);
    }

    private static int ReadAsn1Length(ReadOnlySpan<byte> data, ref int index)
    {
        byte b = data[index++];
        if ((b & 0x80) == 0) return b;

        int lenBytes = b & 0x7F;
        int length = 0;
        for (int i = 0; i < lenBytes; i++)
            length = (length << 8) | data[index++];
        return length;
    }

    public static void Decrypt(ReadOnlySpan<byte> encrypted, Span<byte> output)
    {
        Debug.Assert(encrypted.Length is >= 80);
        Debug.Assert(output.Length is >= 56);

        var (modulus, exponent) = Publickey.Value;

        BigInteger cipher1 = new(encrypted[..40], isUnsigned: true, isBigEndian: false);
        BigInteger cipher2 = new(encrypted[40..], isUnsigned: true, isBigEndian: false);

        var plain1 = BigInteger.ModPow(cipher1, exponent, modulus);
        var plain2 = BigInteger.ModPow(cipher2, exponent, modulus);

        using MemoryStream ms = new(256);
        ms.Write(plain1.ToByteArray(isUnsigned: true));
        ms.Write(plain2.ToByteArray(isUnsigned: true));

        Debug.Assert(ms.Length is 56);
        ms.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        ms.Read(output);
    }
}
