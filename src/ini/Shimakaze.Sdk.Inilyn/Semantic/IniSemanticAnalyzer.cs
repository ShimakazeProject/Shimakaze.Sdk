using System.Runtime.CompilerServices;

using Shimakaze.Sdk.Inilyn.Symbols;
using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;

namespace Shimakaze.Sdk.Inilyn.Semantic;

/// <summary>
/// INI 语义分析器。
/// </summary>
/// <remarks>
/// <para>
/// 支持跨文件分析：在多个文件的符号表基础上整理依赖树，检测循环引用，
/// 并按每个文件分别进行 Mixin 展开（可引用其他文件的节），
/// 产出展平后的 <see cref="IniSemanticModel"/>。
/// </para>
/// </remarks>
public sealed class IniSemanticAnalyzer
{
    private readonly List<Diagnostic> _diagnostics = [];
    private IReadOnlyList<IniSymbolTable> _files = [];
    private readonly List<MixinDependencyNode> _nodes = [];
    private Dictionary<string, List<MixinDependencyNode>> _byName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 对多个文件（按文件名排序）执行语义分析。
    /// </summary>
    /// <param name="files">符号表列表（按文件名排序）。</param>
    /// <returns>与输入顺序对齐的、每个文件展平后的语义模型（Mixin 已展开）。</returns>
    public static IReadOnlyList<IniSemanticModel> AnalyzeFiles(IReadOnlyList<IniSymbolTable> files)
    {
        IniSemanticAnalyzer analyzer = new();
        return analyzer.AnalyzeCore(files);
    }

    private List<IniSemanticModel> AnalyzeCore(IReadOnlyList<IniSymbolTable> files)
    {
        _files = files;

        // 1. 跨文件整理出依赖树（Mixin 引用关系解析为节点图）
        BuildDependencyTree();

        // 2. 验证 Mixin 引用（跨文件查找是否存在）
        ValidateMixinReferences();

        // 3. 基于依赖树检测循环引用
        DetectCircularReferences();

        // 4. 分别针对每个文件进行 Mixin 展开（可跨文件引用）
        ExpandAll();

        // 5. 按文件归组，产出每个文件不带 Mixin 的语义模型
        List<MixinDependencyNode>[] perFile = new List<MixinDependencyNode>[_files.Count];
        for (int i = 0; i < perFile.Length; i++)
        {
            perFile[i] = [];
        }

        foreach (var node in _nodes)
        {
            perFile[node.FileIndex].Add(node);
        }

        List<IniSemanticModel> models = [];
        for (int i = 0; i < _files.Count; i++)
        {
            var sections = perFile[i]
                .Select(n => new IniSemanticSection(n.Section.Name, n.Expanded!))
                .ToList();

            models.Add(new IniSemanticModel
            {
                Sections = sections,
                GlobalKeys = FlattenGlobalKeys(_files[i]),
                Diagnostics = _diagnostics,
            });
        }

        return models;
    }

    private void BuildDependencyTree()
    {
        _nodes.Clear();
        _byName = new Dictionary<string, List<MixinDependencyNode>>(StringComparer.OrdinalIgnoreCase);

        // 先为每个文件中的每个节建立节点（按文件顺序）
        for (int i = 0; i < _files.Count; i++)
        {
            foreach (var section in _files[i].Sections.Values)
            {
                MixinDependencyNode node = new() { Section = section, FileIndex = i };
                _nodes.Add(node);

                if (!_byName.TryGetValue(section.Name, out var list))
                {
                    list = [];
                    _byName[section.Name] = list;
                }

                list.Add(node);
            }
        }

        // 再解析 Mixin 引用，填充依赖关系（保持从左到右的覆盖顺序）
        foreach (var node in _nodes)
        {
            foreach (var mixinRef in node.Section.MixinRefs)
            {
                var dependency = Resolve(node, mixinRef.ReferencedSection);
                if (dependency is null)
                {
                    // 引用不存在的节，稍后在 ValidateMixinReferences 报告
                    continue;
                }

                node.Dependencies.Add((dependency, mixinRef));
            }
        }
    }

    /// <summary>
    /// 解析 Mixin 引用：同文件内的节优先；否则取文件名顺序下第一个同名的节（跨文件）。
    /// </summary>
    private MixinDependencyNode? Resolve(MixinDependencyNode node, string referencedName)
    {
        if (!_byName.TryGetValue(referencedName, out var candidates))
        {
            return null;
        }

        foreach (var candidate in candidates)
        {
            if (candidate.FileIndex == node.FileIndex)
            {
                return candidate;
            }
        }

        return candidates[0];
    }

    private void ValidateMixinReferences()
    {
        foreach (var node in _nodes)
        {
            var file = _files[node.FileIndex];
            foreach (var mixinRef in node.Section.MixinRefs)
            {
                if (!_byName.ContainsKey(mixinRef.ReferencedSection))
                {
                    _diagnostics.Add(
                        Diagnostic.Create(
                            Diagnostics.MixinSectionNotFound,
                            mixinRef.StartLine,
                            mixinRef.StartColumn,
                            mixinRef.EndLine,
                            mixinRef.EndColumn,
                            file.SourceFileName,
                            mixinRef.ReferencedSection
                        )
                    );
                }
            }
        }
    }

    private void DetectCircularReferences()
    {
        HashSet<MixinDependencyNode> visited = new(ReferenceComparer<MixinDependencyNode>.Instance);

        foreach (var node in _nodes)
        {
            if (!visited.Contains(node))
            {
                HashSet<MixinDependencyNode> inProgress = new(ReferenceComparer<MixinDependencyNode>.Instance);
                List<MixinDependencyNode> path = [];
                DetectCircularReferencesCore(node, inProgress, visited, path);
            }
        }
    }

    private void DetectCircularReferencesCore(
        MixinDependencyNode current,
        HashSet<MixinDependencyNode> inProgress,
        HashSet<MixinDependencyNode> visited,
        List<MixinDependencyNode> path)
    {
        inProgress.Add(current);
        path.Add(current);

        foreach (var (dependency, mixinRef) in current.Dependencies)
        {
            if (inProgress.Contains(dependency))
            {
                // 发现从当前节点回到依赖节点的回边，构成循环：完整路径 = 依赖 → 当前（→ 依赖）
                int cycleStart = path.IndexOf(dependency);
                string cyclePath = string.Join(
                    " -> ",
                    path.Skip(cycleStart).Append(dependency).Select(n => n.Section.Name));

                _diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.MixinCircularReference,
                        mixinRef.StartLine,
                        mixinRef.StartColumn,
                        mixinRef.EndLine,
                        mixinRef.EndColumn,
                        _files[current.FileIndex].SourceFileName,
                        cyclePath
                    )
                );
                continue;
            }

            if (visited.Contains(dependency))
            {
                // 该依赖已处理完毕，其内部循环也已报告过
                continue;
            }

            DetectCircularReferencesCore(dependency, inProgress, visited, path);
        }

        path.RemoveAt(path.Count - 1);
        inProgress.Remove(current);
        visited.Add(current);
    }

    private void ExpandAll()
    {
        foreach (var node in _nodes)
        {
            ExpandMixin(node);
        }
    }

    private static List<IniSemanticKeyValue> ExpandMixin(MixinDependencyNode node)
    {
        if (node.Expanded is not null)
        {
            return node.Expanded;
        }

        // 依赖树中已保证无循环引用，无需在展平时再打断递归
        List<IniSemanticKeyValue> keyValues = [];

        // 从左到右递归展开 Mixin 依赖（后者覆盖前者，传递继承祖父节内容）
        foreach (var (dependency, mixinRef) in node.Dependencies)
        {
            foreach (var key in ExpandMixin(dependency))
            {
                keyValues.Add(new IniSemanticKeyValue(key.Key, key.Value, mixinRef.ReferencedSection));
            }
        }

        // 当前节的键值对覆盖 Mixin 来源
        foreach (var key in node.Section.Keys.Values)
        {
            // 移除已存在的同名键（从 Mixin 来源）
            keyValues.RemoveAll(kv => string.Equals(kv.Key, key.Name, StringComparison.OrdinalIgnoreCase));

            string value = GetKeyValueText(key);
            if (string.IsNullOrEmpty(value))
            {
                // 空值：覆盖并移除该键（含 Mixin 来源的同名键）
                continue;
            }

            keyValues.Add(new IniSemanticKeyValue(key.Name, value));
        }

        node.Expanded = [.. keyValues];
        return node.Expanded;
    }

    private static List<IniSemanticKeyValue> FlattenGlobalKeys(IniSymbolTable symbolTable)
    {
        List<IniSemanticKeyValue> result = [];

        foreach (var key in symbolTable.GlobalKeys.Values)
        {
            result.Add(new IniSemanticKeyValue(key.Name, GetKeyValueText(key)));
        }

        return result;
    }

    private static string GetKeyValueText(IniKeySymbol key)
    {
        if (key.DeclaredAt is IniKeyValueEntry entry)
        {
            return entry.Value.Text;
        }

        return string.Empty;
    }

    /// <summary>
    /// 基于引用相等性的比较器（netstandard2.0 无 <see cref="ReferenceEqualityComparer"/>）。
    /// </summary>
    private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceComparer<T> Instance = new();

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }

    /// <summary>
    /// Mixin 依赖树节点：代表某个文件中的一个节及其被 Mixin 引用的依赖。
    /// </summary>
    private sealed class MixinDependencyNode
    {
        /// <summary>
        /// 对应的节符号。
        /// </summary>
        public required IniSectionSymbol Section { get; init; }

        /// <summary>
        /// 所属文件的索引（对应 <see cref="_files"/>）。
        /// </summary>
        public required int FileIndex { get; init; }

        /// <summary>
        /// 依赖列表（从左到右），包含指向依赖节的引用符号以便定位诊断位置。
        /// </summary>
        public List<(MixinDependencyNode Dependency, IniMixinSymbol Ref)> Dependencies { get; } = [];

        /// <summary>
        /// 展平缓存：首次展开后缓存结果。
        /// </summary>
        public List<IniSemanticKeyValue>? Expanded { get; set; }
    }
}