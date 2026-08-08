namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 注册表节声明：<c>[Section]</c> 的值是该节定义的合法名称。
/// </summary>
/// <param name="Section">注册表节名。</param>
/// <param name="Element">元素类型（可跨组，如 <c>Art.Animation</c>）。</param>
public sealed record class InilynRegistryDeclaration(string Section, string Element);

/// <summary>
/// 枚举节声明：<c>[Section]</c> 的键是枚举成员（<c>Enum</c> 可省略），值是 <c>ValueType</c>。
/// </summary>
/// <param name="Section">节名。</param>
/// <param name="Enum">可选，键必须来自该枚举。</param>
/// <param name="ValueType">值的类型。</param>
/// <param name="List">可选，值的分隔符。</param>
public sealed record class InilynEnumSectionDeclaration(string Section, string? Enum, string ValueType = "string", string? List = null);

/// <summary>
/// 全局节声明：不参与 TreeShaking。
/// </summary>
/// <param name="Section">节名。</param>
/// <param name="Type">可选，该节遵循的节定义名；省略时默认为节名本身。</param>
public sealed record class InilynGlobalDeclaration(string Section, string? Type = null);

/// <summary>
/// 发现规则：处理没有注册表的类型。
/// </summary>
/// <param name="Target">被发现节的类型（点号=跨组，无点号=同组）。</param>
/// <param name="From">拥有方类型（仅跨组需要）。</param>
/// <param name="ResolveKey">用哪个键的值作为目标节名。</param>
/// <param name="Fallback">可选，<c>self</c> 表示 ResolveKey 缺失时用自身节名。</param>
public sealed record class InilynDiscoveryRule(string Target, string? From = null, string? ResolveKey = null, string? Fallback = null);
