using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;
using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn.Symbols;

/// <summary>
/// 从语法树构建符号表。
/// </summary>
public sealed class IniSymbolBuilder
{
    private IniSymbolTable _symbolTable = new();
    private string? _filePath;
    private SourceText? _sourceText;
    private readonly HashSet<IniSyntaxNode> _reportedDuplicateNodes = [];

    /// <summary>
    /// 从语法树构建符号表（创建新的符号表）。
    /// </summary>
    /// <param name="syntaxTree">语法树。</param>
    /// <returns>构建完成的符号表。</returns>
    public static IniSymbolTable Build(IniSyntaxTree syntaxTree)
    {
        IniSymbolBuilder builder = new()
        {
            _filePath = syntaxTree.SourceText.FileName,
            _sourceText = syntaxTree.SourceText,
        };
        return builder.BuildCore(syntaxTree);
    }

    /// <summary>
    /// 从语法树构建符号表，合并到已有的符号表中。
    /// </summary>
    /// <param name="syntaxTree">语法树。</param>
    /// <param name="target">目标符号表（合并目标）。</param>
    /// <returns>合并后的符号表。</returns>
    public static IniSymbolTable Build(IniSyntaxTree syntaxTree, IniSymbolTable target)
    {
        IniSymbolBuilder builder = new()
        {
            _symbolTable = target,
            _filePath = syntaxTree.SourceText.FileName,
            _sourceText = syntaxTree.SourceText,
        };
        return builder.BuildCore(syntaxTree);
    }

    private IniSymbolTable BuildCore(IniSyntaxTree syntaxTree)
    {
        if (syntaxTree.Root is not IniCompilationUnit compilationUnit)
        {
            return _symbolTable;
        }

        foreach (var entry in compilationUnit.Entries)
        {
            ProcessEntry(entry, currentSection: null);
        }

        return _symbolTable;
    }

    private (int Line, int Column) GetPosition(int position)
    {
        if (_sourceText is not null)
        {
            return _sourceText.GetPosition(position);
        }
        return (0, 0);
    }

    private void ProcessEntry(IniSyntaxNode node, IniSectionSymbol? currentSection)
    {
        switch (node)
        {
            case IniSectionDecl section:
                ProcessSection(section);
                break;

            case IniKeyValueEntry keyValue:
                ProcessKeyValue(keyValue, currentSection);
                break;

            case IniPreprocessorDirective:
                break;
        }
    }

    private void ProcessSection(IniSectionDecl section)
    {
        string sectionName = section.Name.Text;

        IniSectionSymbol symbol;
        if (_symbolTable.Sections.TryGetValue(sectionName, out var existing))
        {
            // 重复节：在第一次与本次声明处同时报告
            var (line, column) = GetPosition(section.Name.Start);
            var (endLine, endColumn) = GetPosition(section.Name.End);
            ReportDuplicate(Diagnostics.DuplicateSection, sectionName, existing.DeclaredAt, section, line, column, endLine, endColumn);

            // 新建符号指向最新声明并保留已合并的键与 Mixin 引用，
            // 使每个被后续声明覆盖的声明各获得一次警告
            symbol = new(sectionName, section);
            foreach (var kv in existing.Keys)
            {
                symbol.Keys[kv.Key] = kv.Value;
            }
            foreach (var mixinRef in existing.MixinRefs)
            {
                symbol.MixinRefs.Add(mixinRef);
            }
            _symbolTable.Sections[sectionName] = symbol;
        }
        else
        {
            symbol = new(sectionName, section);
            _symbolTable.Sections[sectionName] = symbol;
        }

        // 处理 Mixin 引用
        if (section.MixinClause is not null)
        {
            foreach (var mixinRef in section.MixinClause.References)
            {
                string referencedName = mixinRef.Name.Text;
                var (sl, sc) = GetPosition(mixinRef.Start);
                var (el, ec) = GetPosition(mixinRef.End);
                IniMixinSymbol mixinSymbol = new(sectionName, referencedName, mixinRef, sl, sc, el, ec);
                symbol.MixinRefs.Add(mixinSymbol);
                _symbolTable.AllMixinRefs.Add(mixinSymbol);
            }
        }

        // 处理节内的子条目
        foreach (var child in section.Children)
        {
            ProcessEntry(child, symbol);
        }
    }

    private void ProcessKeyValue(IniKeyValueEntry keyValue, IniSectionSymbol? currentSection)
    {
        string keyName = keyValue.Key.Text;

        if (currentSection is not null)
        {
            // 检测同节内重复键：同时在第一次与本次出现处报告
            if (currentSection.Keys.TryGetValue(keyName, out var existingKey))
            {
                var (line, column) = GetPosition(keyValue.Key.Start);
                var (endLine, endColumn) = GetPosition(keyValue.Key.End);
                ReportDuplicate(Diagnostics.DuplicateKey, keyName, existingKey.DeclaredAt, keyValue, line, column, endLine, endColumn);
            }

            IniKeySymbol keySymbol = new(keyName, keyValue);
            currentSection.Keys[keyName] = keySymbol;
        }
        else
        {
            // 全局键
            if (_symbolTable.GlobalKeys.TryGetValue(keyName, out var existingKey))
            {
                var (line, column) = GetPosition(keyValue.Key.Start);
                var (endLine, endColumn) = GetPosition(keyValue.Key.End);
                ReportDuplicate(Diagnostics.DuplicateKey, keyName, existingKey.DeclaredAt, keyValue, line, column, endLine, endColumn);
            }

            IniKeySymbol keySymbol = new(keyName, keyValue);
            _symbolTable.GlobalKeys[keyName] = keySymbol;
        }
    }

    private void ReportDuplicate(
        DiagnosticDescriptor descriptor,
        string name,
        IniSyntaxNode existingDeclaredAt,
        IniSyntaxNode newDeclaredAt,
        int line,
        int column,
        int endLine,
        int endColumn)
    {
        // 已存在的那次出现只在首次参与重复时报告，避免中间出现被重复警告
        if (_reportedDuplicateNodes.Add(existingDeclaredAt))
        {
            switch (existingDeclaredAt)
            {
                case IniSectionDecl section:
                {
                    var (sl, sc) = GetPosition(section.Name.Start);
                    var (sel, sec) = GetPosition(section.Name.End);
                    _symbolTable.Diagnostics.Add(Diagnostic.Create(descriptor, sl, sc, sel, sec, _filePath, name));
                    break;
                }
                case IniKeyValueEntry kv:
                {
                    var (sl, sc) = GetPosition(kv.Key.Start);
                    var (sel, sec) = GetPosition(kv.Key.End);
                    _symbolTable.Diagnostics.Add(Diagnostic.Create(descriptor, sl, sc, sel, sec, _filePath, name));
                    break;
                }
            }
        }

        _reportedDuplicateNodes.Add(newDeclaredAt);
        _symbolTable.Diagnostics.Add(Diagnostic.Create(descriptor, line, column, endLine, endColumn, _filePath, name));
    }
}
