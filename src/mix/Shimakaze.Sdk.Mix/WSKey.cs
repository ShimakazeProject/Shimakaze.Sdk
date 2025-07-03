using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Numerics;


namespace Shimakaze.Sdk.Mix;

internal static class WSKey
{
    private static readonly Lazy<BigInteger> Modulus = new(() => ToBigInteger("AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V"u8));
    private static readonly Lazy<BigInteger> PublicKeyExponent = new(() => 0x10001u);
    private static readonly Lazy<BigInteger> PrivateKeyExponent = new(() => ToBigInteger("AigKVje8mROcR8QixnxUEF5b29Curkq01DNDWCdOG99XBqH79OaCiTCB"u8));

    private static BigInteger ToBigInteger(ReadOnlySpan<byte> base64)
    {
        var len = Base64.GetMaxDecodedFromUtf8Length(base64.Length);
        Span<byte> data = stackalloc byte[len];
        var status = Base64.DecodeFromUtf8(base64, data, out _, out len);
        Debug.Assert(status is OperationStatus.Done);
        data = data[..len];

        int index = 0;

        if (data[index++] != 0x02)
            throw new FormatException("Expected INTEGER");

        int length = ReadAsn1Length(data, ref index);
        return new(data.Slice(index, length), isBigEndian: true, isUnsigned: true);
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
        Debug.Assert(encrypted.Length is 80);
        Debug.Assert(output.Length is 56);

        var modulus = Modulus.Value;
        var exponent = PublicKeyExponent.Value;

        BigInteger cipher1 = new(encrypted[..40], isUnsigned: true, isBigEndian: false);
        BigInteger cipher2 = new(encrypted[40..], isUnsigned: true, isBigEndian: false);

        var plain1 = BigInteger.ModPow(cipher1, exponent, modulus);
        var plain2 = BigInteger.ModPow(cipher2, exponent, modulus);

        plain1.TryWriteBytes(output, out int written1, isUnsigned: true, isBigEndian: false);
        plain2.TryWriteBytes(output[written1..], out int written2, isUnsigned: true, isBigEndian: false);
        Debug.Assert(written1 + written2 is 56);
    }

    public static void Encrypt(ReadOnlySpan<byte> input, Span<byte> encrypted)
    {
        Debug.Assert(input.Length == 56);
        Debug.Assert(encrypted.Length == 80);

        var modulus = Modulus.Value;
        var exponent = PrivateKeyExponent.Value;

        var split = modulus.GetByteCount() - 1;

        BigInteger part1 = new(input[..split], isUnsigned: true, isBigEndian: true);
        BigInteger part2 = new(input[split..], isUnsigned: true, isBigEndian: true);

        BigInteger cipher1 = BigInteger.ModPow(part1, exponent, modulus);
        BigInteger cipher2 = BigInteger.ModPow(part2, exponent, modulus);

        cipher1.TryWriteBytes(encrypted, out int written1, isUnsigned: true, isBigEndian: false);
        cipher2.TryWriteBytes(encrypted[written1..], out int written2, isUnsigned: true, isBigEndian: false);
        Debug.Assert(written1 + written2 is 80);
    }
}
