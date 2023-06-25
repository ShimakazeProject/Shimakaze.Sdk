using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// PreprocessorVariablesBuilder
/// </summary>
internal sealed class PreprocessorVariablesBuilder : IPreprocessorVariablesBuilder
{
    private readonly IPreprocessorVariables _variables = new PreprocessorVariables();
    private readonly IServiceProvider _service;

    public PreprocessorVariablesBuilder(IServiceProvider service)
    {
        _service = service;
    }

    public IPreprocessorVariablesBuilder AddDefine(string define)
    {
        _variables.Defines.Add(define);
        return this;
    }

    public IPreprocessorVariablesBuilder AddDefines(params string[] defines) => AddDefines(defines as IEnumerable<string>);

    public IPreprocessorVariablesBuilder AddDefines(IEnumerable<string> defines)
    {
        foreach (var define in defines)
            AddDefine(define);

        return this;
    }

    public IPreprocessorVariables Build() => _variables;
}