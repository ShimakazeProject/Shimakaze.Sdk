using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Ini.Directives;
using Shimakaze.Sdk.Ra2lyn.Ini.Lowering;
using Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

namespace Shimakaze.Sdk.Ra2lyn.Ini;

/// <summary>
/// INI 前端门面：预处理、词法分析、语法分析与降级四阶段的固定编排。
/// </summary>
/// <remarks>
/// 预处理阶段按宏集合求值条件指令并剔除未生效内容；其诊断与语法、降级诊断一并出现在
/// <see cref="FrontendResult.Diagnostics"/> 中。
/// </remarks>
public sealed class IniFrontend
{
    private readonly IniParseOptions _options;

    /// <summary>
    /// 使用默认解析选项初始化门面。
    /// </summary>
    public IniFrontend()
        : this(new())
    {
    }

    /// <summary>
    /// 初始化门面。
    /// </summary>
    /// <param name="options">门面的默认解析选项。</param>
    public IniFrontend(IniParseOptions options)
        => _options = options;

    /// <summary>
    /// 门面的默认解析选项。
    /// </summary>
    public IniParseOptions Options
        => _options;

    /// <summary>
    /// 按门面的默认选项解析一个 INI 源文件。
    /// </summary>
    /// <param name="source">待解析的源文件。</param>
    /// <param name="group">本文件所属的规则组，由调用方决定，不从扩展名推断。</param>
    public FrontendResult Parse(SourceFile source, string group)
        => Run(source, group, _options.LowerToIr, _options);

    /// <summary>
    /// 解析一个 INI 源文件。
    /// </summary>
    /// <param name="source">待解析的源文件。</param>
    /// <param name="group">本文件所属的规则组，由调用方决定，不从扩展名推断。</param>
    /// <param name="options">本次解析使用的选项；契约路径要求显式给出选项。</param>
    /// <returns>
    /// 前端结果；<paramref name="options"/> 中的 <see cref="IniParseOptions.LowerToIr"/> 为
    /// <see langword="false"/> 时实体列表为空。
    /// </returns>
    public FrontendResult Parse(SourceFile source, string group, IniParseOptions options)
    {
        // 显式选项与门面默认选项的“降级开关”共同决定结果：显式选项优先。
        bool lowerToIr = options.LowerToIr && _options.LowerToIr;
        return Run(source, group, lowerToIr, options);
    }

    private static FrontendResult Run(SourceFile source, string group, bool lowerToIr, IniParseOptions options)
    {
        PreprocessResult preprocess = IniPreprocessor.Process(source, new DefineSet(options.Defines));
        IniSyntaxTree tree = IniParser.Parse(source, preprocess.Diagnostics, preprocess.Tokens, preprocess.TemplateModes);
        if (lowerToIr is false)
            return new(group, [source], [], tree.Diagnostics);

        return new AstToIrLowerer(group, preprocess.Pragmas).Lower(tree, source);
    }
}
