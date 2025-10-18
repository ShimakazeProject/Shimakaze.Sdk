using System.Numerics;

namespace Shimakaze.Sdk.Mix;

internal readonly struct BigIntegerPolyfill
{
    private readonly BigInteger _value;

    private BigIntegerPolyfill(BigInteger value) => _value = value;
    public BigIntegerPolyfill(ReadOnlySpan<byte> value, bool isUnsigned, bool isBigEndian)
#if NETSTANDARD
    {
        if (value.IsEmpty)
        {
            _value = 0;
            return;
        }

        byte[] data = value.ToArray();
        // 将大端数据转换为小端
        if (isBigEndian)
            Array.Reverse(data);

        if (!isUnsigned)
        {
            _value = new(data);
            return;
        }

        // 确保数据转换成大数字时为正整数
        byte tmp = data[^1];
        if ((tmp & 0b1000_0000) is 0)
        {
            _value = new(data);
            return;
        }

        // 如果是负数 可以在数据最高位补一位符号
        data = [.. data, 0];

        _value = new(data);
    }
#else
        : this(new BigInteger(value, isUnsigned, isBigEndian))
    {
    }
#endif

    public static BigIntegerPolyfill ModPow(BigIntegerPolyfill value, BigIntegerPolyfill exponent, BigIntegerPolyfill modulus)
        => BigInteger.ModPow(value, exponent, modulus);

    public bool TryWriteBytes(Span<byte> destination, out int bytesWritten, bool isUnsigned, bool isBigEndian)
    {
#if NETSTANDARD
        if (isUnsigned && _value is { Sign: < 0 })
            throw new OverflowException();

        // 小字节序的数据
        Span<byte> data = _value.ToByteArray();

        // 去掉高位0
        if (data[^1] is 0)
            data = data[..^1];

        // 如果需要一个大字节序的数据 这里还需要反转一下
        if (isBigEndian)
            data.Reverse();

        // 计算出写入的字节数
        bytesWritten = Math.Min(data.Length, destination.Length);

        // 将结果写出
        data.CopyTo(destination);

        // 如果没写完 返回 false
        return bytesWritten == data.Length;
#else
        return _value.TryWriteBytes(destination, out bytesWritten, isUnsigned, isBigEndian);
#endif
    }

    public int GetByteCount()
    {
#if NETSTANDARD
        // 小字节序的数据
        Span<byte> data = _value.ToByteArray();

        // 去掉高位0
        if (data[^1] is 0)
            data = data[..^1];

        return data.Length;
#else
        return _value.GetByteCount();
#endif
    }

    public static implicit operator BigIntegerPolyfill(uint value) => new(value);
    public static implicit operator BigIntegerPolyfill(BigInteger value) => new(value);
    public static implicit operator BigInteger(BigIntegerPolyfill value) => value._value;
}
