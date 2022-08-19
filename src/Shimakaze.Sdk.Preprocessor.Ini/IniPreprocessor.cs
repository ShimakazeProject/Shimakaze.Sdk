namespace Shimakaze.Sdk.Preprocessor.Ini;

/// <summary>
/// 预处理器
/// </summary>
public sealed class IniPreprocessor : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// 变量表
    /// </summary>
    /// <remarks>
    /// 预处理器的所有变量保存在这里
    /// </remarks>
    public Dictionary<string, object> Variables { get; } = new();
    /// <summary>
    /// 获取预处理器变量
    /// </summary>
    /// <typeparam name="T">变量的类型</typeparam>
    /// <param name="key">变量名</param>
    /// <returns>变量的值</returns>
    public T GetVariable<T>(string key) => (T)Variables[key];
    /// <summary>
    /// 断言预处理器变量
    /// </summary>
    /// <param name="key">变量名</param>
    /// <returns>变量对象</returns>
    /// <exception cref="VariableNotInitializeException">当变量不存在时抛出异常</exception>
    public object AssertVariable(string key) => Variables.ContainsKey(key)
        ? Variables[key]
        : throw new VariableNotInitializeException($"{key} not initialized");

    /// <summary>
    /// [反射] 初始化预处理器
    /// </summary>
    /// <param name="output">输出流</param>
    /// <param name="workdir">工作目录</param>
    /// <param name="defines">定义</param>
    public async Task InitializeAsync(TextWriter output, string workdir, params string[] defines)
    {
        await Task.Yield();

        // 初始化输出流
        Variables[PreprocessorVariableNames.OutputStream_TextWriter] = output;

        // 初始化工作目录栈
        Variables[PreprocessorVariableNames.WorkingDirectory_Stack_String] = new Stack<string>();
        GetVariable<Stack<string>>(PreprocessorVariableNames.WorkingDirectory_Stack_String).Push(workdir);
        Debug.WriteLine($"Push WorkingDirectory: {workdir}");

        Variables[PreprocessorVariableNames.CurrentFile_Stack_String] = new Stack<string>();
        Variables[PreprocessorVariableNames.DefineStack_Stack_String] = new Stack<string>();
        Variables[PreprocessorVariableNames.Defines_HashSet_String] = new HashSet<string>(defines);
        Variables[PreprocessorVariableNames.WriteOutput_Boolean] = true;

        // 通过反射取得所有支持的命令
        Variables[PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand] = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(i => i.GetTypes())
            .Where(i => i.GetInterfaces().Contains(typeof(IPreprocessorCommand)))
            .Select(i => (IPreprocessorCommand)Activator.CreateInstance(i)!)
            .ToDictionary(i => i.Command);
        Debug.WriteLine($"Find Commands: [\n  {string.Join(", \n  ",
            GetVariable<Dictionary<string, IPreprocessorCommand>>(
                PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand).Keys
                )}\n]");

        // 初始化所有命令
        await GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand).Values
            .EachAsync(async i => await i.InitializeAsync(this));
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public async Task ExecuteAsync(string filePath)
    {
        GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile_Stack_String).Push(filePath);
        await File.OpenText(filePath).Using(ExecuteAsync);
        GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile_Stack_String).Pop();
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    /// <param name="reader">输入流</param>
    private async Task ExecuteAsync(TextReader reader)
    {
        Func<bool> check = reader is StreamReader sr
            ? (() => !sr.EndOfStream)
            : (() => reader.Peek() >= 0);
        while (check())
        {
            string? raw = await reader.ReadLineAsync();
            await ParseLineAsync(raw!).ConfigureAwait(false);
        }

        await GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand)
            .Values
            .Where(i => i.IsPostProcessing)
            .EachAsync(i => i.PostProcessingAsync(this));
    }

    private async Task ParseLineAsync(string raw)
    {
        AssertVariable(PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand);
        AssertVariable(PreprocessorVariableNames.OutputStream_TextWriter);

        // Trim
        string ts = raw.TrimStart();

        if (!ts.StartsWith("#"))
        {
            if (GetVariable<bool>(PreprocessorVariableNames.WriteOutput_Boolean))
                await GetVariable<TextWriter>(PreprocessorVariableNames.OutputStream_TextWriter).WriteLineAsync(raw).ConfigureAwait(false);

            Debug.WriteLine($"Write {GetVariable<bool>(PreprocessorVariableNames.WriteOutput_Boolean)}: {raw}");
            return;
        }

        await ParseCommand(ts[1..].Trim()).Then(InvokeCommand);
    }

    private async Task InvokeCommand(string command, params string[] args)
    {
        if (GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand)
            .TryGetValue(command, out var commandHandler))
        {
            Debug.WriteLine($"Invoke: {command} {string.Join(" ", args)}");
            await commandHandler.ExecuteAsync(args, this).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="raw">预处理器命令字符串</param>
    /// <returns>命令</returns>
    private static (string command, string[] args) ParseCommand(string raw)
    {
        // split by space and do not split by quotes
        List<string> list = new();
        bool quotes = false;
        StringBuilder sb = new();
        foreach (char ch in raw)
        {
            switch (ch)
            {
                case '\'':
                case '"':
                    quotes = !quotes;
                    break;
                case ' ':
                    if (!quotes)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        list.Add(sb.ToString());

        string command = list[0];
        list.RemoveAt(0);
        return (command, list.ToArray());
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Variables.Values
                    .Where(v => v is IDisposable)
                    .Select(v => (IDisposable)v)
                    .Each(v => v.Dispose());

                GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames.Commands_Dictionary_String_IPreprocessorCommand)
                    .Where(i => i.Value is IDisposable)
                    .Each(i => ((IDisposable)i.Value).Dispose());
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// 对象终结器
    /// </summary>
    ~IniPreprocessor()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
