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
    /// [反射] 初始化预处理器
    /// </summary>
    /// <param name="output">输出流</param>
    /// <param name="workdir">工作目录</param>
    /// <param name="sourceFileName">需要被处理的源文件</param>
    /// <param name="defines">定义</param>
    /// <param name="extensions">自动识别的扩展名</param>
    public async Task InitializeAsync(
        TextWriter output,
        string workdir,
        IEnumerable<FileInfo> sourceFileName,
        IEnumerable<string> defines,
        IEnumerable<string> extensions
    )
    {
        // 初始化输出流
        Variables[PreprocessorVariableNames.OutputStream] = output;

        // 初始化工作目录栈
        Variables[PreprocessorVariableNames.WorkingDirectory] = new Stack<string>();
        GetVariable<Stack<string>>(PreprocessorVariableNames.WorkingDirectory).Push(workdir);
        Debug.WriteLine($"Push WorkingDirectory: {workdir}");

        Variables[PreprocessorVariableNames.CurrentFile] = new Stack<string>();
        Variables[PreprocessorVariableNames.DefineStack] = new Stack<string>();
        Variables[PreprocessorVariableNames.Defines] = new HashSet<string>(defines);
        Variables[PreprocessorVariableNames.Sources] = new HashSet<FileInfo>(sourceFileName);
        Variables[PreprocessorVariableNames.WriteOutput] = true;
        Variables[PreprocessorVariableNames.Extensions] = extensions;

        // 通过反射取得所有支持的命令
        Variables[PreprocessorVariableNames.Commands] = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(i => i.GetTypes())
            .Where(i => i.GetInterfaces().Contains(typeof(IPreprocessorCommand)))
            .Select(i => (IPreprocessorCommand)Activator.CreateInstance(i)!)
            .ToDictionary(i => i.Command);

        Debug.WriteLine(
            $"Find Commands: [\n  {string.Join(", \n  ", GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames.Commands).Keys)}\n]");

        // 初始化所有命令
        await GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames
                .Commands).Values
            .EachAsync(async i => await i.InitializeAsync(this));
    }

    public FileInfo GetFileFromSourceFileList(string name, string currentFolder)
    {
        var path = Path.Combine(currentFolder, name);
        if (!File.Exists(path))
        {
            foreach (var ext in GetVariable<IEnumerable<string>>(PreprocessorVariableNames.Extensions))
            {
                path = Path.Combine(currentFolder, $"{name}{ext}");
                if (File.Exists(path))
                    break;
            }
        }

        if (!File.Exists(path))
            throw new FileNotFoundException($"cannot found ${name}");

        var file = new FileInfo(path);

        var list = GetVariable<HashSet<FileInfo>>(PreprocessorVariableNames.Sources);
        if (list.All(i => i.FullName != file.FullName))
            throw new FileNotFoundException($"file \"{file.FullName}\" are not exists in Source File List.\n" +
                                            $"{string.Join('\n', list.Select(i => $"\"{i.FullName}\""))}");

        return file;
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    /// <param name="entry">文件路径</param>
    public async Task ExecuteAsync(FileInfo entry)
    {
        GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile).Push(entry.FullName);
        using var sr = entry.OpenText();
        await ExecuteAsync(sr);
        sr.Dispose();
        GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile).Pop();
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

        await GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames
                .Commands)
            .Values
            .Where(i => i.IsPostProcessing)
            .EachAsync(i => i.PostProcessingAsync(this));
    }

    private async Task ParseLineAsync(string raw)
    {
        // Trim
        string ts = raw.TrimStart();

        if (!ts.StartsWith("#"))
        {
            if (GetVariable<bool>(PreprocessorVariableNames.WriteOutput))
                await GetVariable<TextWriter>(PreprocessorVariableNames.OutputStream).WriteLineAsync(raw)
                    .ConfigureAwait(false);

            Debug.WriteLine($"Write {GetVariable<bool>(PreprocessorVariableNames.WriteOutput)}: {raw}");
            return;
        }

        await ParseCommand(ts[1..].Trim()).Then(InvokeCommand);
    }

    private async Task InvokeCommand(string command, params string[] args)
    {
        if (GetVariable<Dictionary<string, IPreprocessorCommand>>(PreprocessorVariableNames
                .Commands)
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
                    .OfType<IDisposable>()
                    .Each(v => v.Dispose());

                GetVariable<Dictionary<string, IPreprocessorCommand>>(
                        PreprocessorVariableNames.Commands)
                    .Each(i => (i.Value as IDisposable)?.Dispose());
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