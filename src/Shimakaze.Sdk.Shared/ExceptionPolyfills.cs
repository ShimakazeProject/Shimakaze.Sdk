// .NET Framework 兼容层：用 C# 14 extension 块为 ArgumentNullException /
// ArgumentOutOfRangeException 补齐 ThrowIf* 静态成员。
// 仅在目标框架为 NETFRAMEWORK 时编译；net10.0 使用 BCL 自带成员。
#if NETFRAMEWORK || NETSTANDARD
using System.Runtime.CompilerServices;

internal static class ExceptionExtensions
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }

    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "参数值不能为负数。");
            }
        }

        public static void ThrowIfGreaterThan(int value, int other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value > other)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "参数值超出范围。");
            }
        }

        public static void ThrowIfGreaterThanOrEqual(int value, int other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value >= other)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "参数值超出范围。");
            }
        }
    }
}

#endif
