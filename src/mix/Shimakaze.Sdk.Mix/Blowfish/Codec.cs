// https://github.com/jdvor/encryption-blowfish

namespace Shimakaze.Sdk.Mix.Blowfish;

/// <summary>
/// Blowfish encryption and decryption on fixed size (length = 8) data block.
/// Codec is a relatively expensive object, because it must construct P-array and S-blocks from provided key.
/// It is expected to be used many times and it is thread-safe.
/// </summary>
public sealed class Codec
{
    private readonly uint[] _p = Init.P;
    private readonly uint[] _s0 = Init.S0;
    private readonly uint[] _s1 = Init.S1;
    private readonly uint[] _s2 = Init.S2;
    private readonly uint[] _s3 = Init.S3;

    /// <summary>
    /// Create codec instance and compute P-array and S-blocks.
    /// </summary>
    /// <param name="key">cipher key; valid size is &lt;8, 448&gt;</param>
    /// <exception cref="ArgumentException">on invalid input</exception>
    public Codec(ReadOnlySpan<byte> key)
    {
        if (key is { IsEmpty: true } or { Length: < 8 or > 448 })
            throw new ArgumentException("invalid key length; not in <8, 448>", nameof(key));

        int j = 0;
        for (int i = 0; i < 18; i++)
        {
            byte d1 = key[j % key.Length];
            byte d2 = key[(j + 1) % key.Length];
            byte d3 = key[(j + 2) % key.Length];
            byte d4 = key[(j + 3) % key.Length];
            uint d = (uint)(((d1 * 256 + d2) * 256 + d3) * 256 + d4);
            _p[i] ^= d;
            j = (j + 4) % key.Length;
        }

        uint xl = 0;
        uint xr = 0;
        for (int i = 0; i < 18; i += 2)
        {
            Encipher(ref xl, ref xr);
            _p[i] = xl;
            _p[i + 1] = xr;
        }

        for (int i = 0; i < 256; i += 2)
        {
            Encipher(ref xl, ref xr);
            _s0[i] = xl;
            _s0[i + 1] = xr;
        }

        for (int i = 0; i < 256; i += 2)
        {
            Encipher(ref xl, ref xr);
            _s1[i] = xl;
            _s1[i + 1] = xr;
        }

        for (int i = 0; i < 256; i += 2)
        {
            Encipher(ref xl, ref xr);
            _s2[i] = xl;
            _s2[i + 1] = xr;
        }

        for (int i = 0; i < 256; i += 2)
        {
            Encipher(ref xl, ref xr);
            _s3[i] = xl;
            _s3[i + 1] = xr;
        }
    }

    private void Encipher(ref uint xl, ref uint xr)
    {
        xl ^= _p[0];
        for (int i = 0; i < 16; i += 2)
        {
            xr = Round(xr, xl, i + 1);
            xl = Round(xl, xr, i + 2);
        }

        xr ^= _p[17];
        (xl, xr) = (xr, xl);
    }

    private void Decipher(ref uint xl, ref uint xr)
    {
        xl ^= _p[17];
        for (int i = 16; i > 0; i -= 2)
        {
            xr = Round(xr, xl, i);
            xl = Round(xl, xr, i - 1);
        }

        xr ^= _p[0];
        (xl, xr) = (xr, xl);
    }

    private uint Round(uint a, uint b, int n)
    {
        uint x = _s0[b >> 24];
        x += _s1[b >> 16 & 0xFF];
        x ^= _s2[b >> 8 & 0xFF];
        x += _s3[b & 0xFF];
        x ^= _p[n];
        return x ^ a;
    }

    /// <summary>
    /// Encrypt data block.
    /// There are no range checks within the method and it is expected that the caller will ensure big enough block.
    /// </summary>
    /// <param name="block">only first 8 bytes are encrypted</param>
    public void Encrypt(Span<byte> block)
    {
        uint xl = (uint)(block[0] << 24 | block[1] << 16 | block[2] << 8 | block[3]);
        uint xr = (uint)(block[4] << 24 | block[5] << 16 | block[6] << 8 | block[7]);
        Encipher(ref xl, ref xr);
        block[0] = (byte)(xl >> 24);
        block[1] = (byte)(xl >> 16);
        block[2] = (byte)(xl >> 8);
        block[3] = (byte)xl;
        block[4] = (byte)(xr >> 24);
        block[5] = (byte)(xr >> 16);
        block[6] = (byte)(xr >> 8);
        block[7] = (byte)xr;
    }

    /// <summary>
    /// Decrypt data block.
    /// There are no range checks within the method and it is expected that the caller will ensure big enough block.
    /// </summary>
    /// <param name="block">only first 8 bytes are decrypted</param>
    public void Decrypt(Span<byte> block)
    {
        uint xl = (uint)(block[0] << 24 | block[1] << 16 | block[2] << 8 | block[3]);
        uint xr = (uint)(block[4] << 24 | block[5] << 16 | block[6] << 8 | block[7]);
        Decipher(ref xl, ref xr);
        block[0] = (byte)(xl >> 24);
        block[1] = (byte)(xl >> 16);
        block[2] = (byte)(xl >> 8);
        block[3] = (byte)xl;
        block[4] = (byte)(xr >> 24);
        block[5] = (byte)(xr >> 16);
        block[6] = (byte)(xr >> 8);
        block[7] = (byte)xr;
    }
}
