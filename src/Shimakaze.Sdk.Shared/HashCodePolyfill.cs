// .NET Framework 兼容层：Polyfill 包未提供 System.HashCode，这里补一个等价实现。
// 仅在目标框架为 NETFRAMEWORK 时编译；net10.0 使用 BCL 自带类型。
#if NETFRAMEWORK || NETSTANDARD

namespace System;

/// <summary>
/// 组合哈希。polyfill for <see cref="System.HashCode"/>。
/// </summary>
internal struct HashCode
{
    private int _hash;

    private static int CombineCore(int h1, int h2) => unchecked(((h1 << 5) + h1) ^ h2);

    public static int Combine<T1>(T1 v1) => v1?.GetHashCode() ?? 0;

    public static int Combine<T1, T2>(T1 v1, T2 v2) => CombineCore(Combine(v1), v2?.GetHashCode() ?? 0);

    public static int Combine<T1, T2, T3>(T1 v1, T2 v2, T3 v3) => CombineCore(Combine(v1, v2), v3?.GetHashCode() ?? 0);

    public static int Combine<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4) => CombineCore(Combine(v1, v2, v3), v4?.GetHashCode() ?? 0);

    public static int Combine<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5) => CombineCore(Combine(v1, v2, v3, v4), v5?.GetHashCode() ?? 0);

    public static int Combine<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6) => CombineCore(Combine(v1, v2, v3, v4, v5), v6?.GetHashCode() ?? 0);

    public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7) => CombineCore(Combine(v1, v2, v3, v4, v5, v6), v7?.GetHashCode() ?? 0);

    public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8) => CombineCore(Combine(v1, v2, v3, v4, v5, v6, v7), v8?.GetHashCode() ?? 0);

    public void Add<T>(T value) => _hash = CombineCore(_hash, value?.GetHashCode() ?? 0);

    public int ToHashCode() => _hash;
}

#endif
