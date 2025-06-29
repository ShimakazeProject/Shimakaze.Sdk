namespace Shimakaze.Sdk.Inilyn;

internal sealed partial class ParserContext
{
    private readonly Dictionary<string, object> _storage = [];

    public bool CanWritable { get; internal set; } = true;

    /// <summary>
    /// 编译器命令
    /// </summary>
    public IReadOnlyDictionary<string, Delegate> CompilerCommands => _compilerCommands.AsReadOnly();

    /// <summary>
    /// 获取或创建新的变量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="creator"></param>
    /// <returns></returns>
    public T GetOrNew<T>(string key, Func<T> creator) where T : notnull
    {
        if (!_storage.TryGetValue(key, out object? value) || value is not T result)
            _storage[key] = result = creator();

        return result;
    }
}
