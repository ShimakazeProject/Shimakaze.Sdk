namespace Shimakaze.Sdk.Ra2lyn.Common.Value;

/// <summary>
/// 一个属性的最终值（<c>ValueResolutionPass</c> 的求值结果）。
/// </summary>
/// <remarks>
/// <para>
/// 三态区分（TODO.md §3.5.8、§4.3.5.2）：
/// </para>
/// <list type="bullet">
/// <item><see cref="IsPresent"/> 为 <see langword="false"/>：key 不存在（例如被 <c>$null</c> 覆盖，或符号表里根本没有）。</item>
/// <item><see cref="HasValue"/> 为 <see langword="true"/>：有真实值，<see cref="Text"/> 就是该值（可能是空字符串）。</item>
/// <item>两者都为 <see langword="false"/>：值被求值成 <c>null</c>（<c>Key=$null</c>），尚未被剔除。</item>
/// </list>
/// <para>
/// null 与空字符串是两种不同的"存在"状态（§3.5.8 / §4.3.5.2），前者等到 <c>NullCleanupPass</c> 才删除。
/// </para>
/// <para>
/// 求值失败（循环引用、越界、无法解析）时 <see cref="Succeeded"/> 为 <see langword="false"/>，
/// <see cref="Text"/> 保留该属性的<b>原始字符串</b>（TODO.md §3.4：出错处保留原始字符串）。
/// </para>
/// </remarks>
public sealed record class ResolvedValue
{
    /// <summary>
    /// 原始值字符串（属性在源文件里写下的文本）。
    /// </summary>
    public required string Original { get; init; }

    /// <summary>
    /// 求值成功时，本属性在最终 entity 里应写入的值；<see langword="null"/> 表示值为 <c>$null</c>。
    /// </summary>
    public string? Text { get; private init; }

    /// <summary>
    /// 求值是否成功；失败时 <see cref="Text"/> 保留 <see cref="Original"/>。
    /// </summary>
    public bool Succeeded { get; private init; }

    /// <summary>
    /// key 是否存在（即"引用它算不算存在"；被 <c>$null</c> 覆盖的 key 为 <see langword="false"/>）。
    /// </summary>
    public bool IsPresent { get; private init; }

    /// <summary>
    /// 是否有具体文本值；为 <see langword="false"/> 时 <see cref="Text"/> 必为 <see langword="null"/>。
    /// </summary>
    public bool HasValue { get; private init; }

    /// <summary>
    /// 构造"有文本值"的结果。
    /// </summary>
    /// <param name="text">最终值。</param>
    /// <param name="original">原始值字符串。</param>
    public static ResolvedValue Present(string text, string original)
        => new() { Succeeded = true, IsPresent = true, HasValue = true, Text = text, Original = original };

    /// <summary>
    /// 构造"值为 null 但仍存在"的结果（<c>$null</c>，TODO.md §3.5.7）。
    /// </summary>
    /// <param name="original">原始值字符串。</param>
    public static ResolvedValue Null(string original)
        => new() { Succeeded = true, IsPresent = true, HasValue = false, Text = null, Original = original };

    /// <summary>
    /// 构造"key 不存在"的结果（TODO.md §3.5.8：被 <c>$null</c> 覆盖）。
    /// </summary>
    public static ResolvedValue Absent()
        => new() { Succeeded = true, IsPresent = false, HasValue = false, Text = null, Original = string.Empty };

    /// <summary>
    /// 构造"求值失败、保留原始字符串"的结果。
    /// </summary>
    /// <param name="original">原始值字符串。</param>
    public static ResolvedValue Failed(string original)
        => new() { Succeeded = false, IsPresent = true, HasValue = true, Text = original, Original = original };

    /// <summary>
    /// 拼接用的文本：不存在或为 null 都按空字符串参与字符串拼接（TODO.md §3.5.8）。
    /// </summary>
    public string ConcatenationText
        => HasValue ? Text! : string.Empty;
}
