namespace Shimakaze.Sdk.Preprocessor;

internal sealed class Preprocessor : IDisposable
{
    public Dictionary<string, IPreprocessorCommand>? Commands { get; private set; }
    private TextWriter? _output;
    private bool _disposedValue;
    private bool _writeOutput = true;

    public Stack<string> WorkingDirectory { get; } = new();

    public Stack<string> DefineStack { get; } = new();

    public HashSet<string> Defines { get; } = new();

    public bool WriteOutput
    {
        get => _writeOutput; set
        {
            _writeOutput = value;
            Debug.WriteLine($"Change WriteOutput: {value}");
        }
    }
    public async Task InitializeAsync(TextWriter output!!, string workdir!!)
    {
        Commands = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(i => i.GetTypes())
            .Where(i => i.GetInterfaces().Contains(typeof(IPreprocessorCommand)))
            .Select(i => (IPreprocessorCommand)Activator.CreateInstance(i)!)
            .ToDictionary(i => i.Command);
        await Commands.Values.EachAsync(async i => await i.InitializeAsync(this));
        _output = output;

        WorkingDirectory.Push(workdir);
        Debug.WriteLine($"Push WorkingDirectory: {workdir}");
    }

    public async Task ExecuteAsync(string iniFilePath!!)
    {
        await using Stream fs = File.OpenRead(iniFilePath);
        using StreamReader sr = new(fs);
        while (!sr.EndOfStream)
        {
            string? raw = await sr.ReadLineAsync();
            if (raw is null)
                throw new Exception("Unexpected end of stream");
            await ParseLineAsync(raw).ConfigureAwait(false);
        }
    }

    internal async Task ParseLineAsync(string raw!!)
    {
        if (Commands is null)
            throw new Exception("Commands not initialized");
        if (_output is null)
            throw new Exception("Output not initialized");

        // Trim
        string ts = raw.TrimStart();

        if (!ts.StartsWith("#"))
        {
            if (WriteOutput)
                await _output.WriteLineAsync(raw).ConfigureAwait(false);

            Debug.WriteLine($"Write {WriteOutput}: {raw}");
            return;
        }

        // Get Preprocessor Command
        var (command, args) = ParseCommand(ts[1..].Trim());
        if (Commands.TryGetValue(command, out var commandHandler))
        {
            Debug.WriteLine($"Invoke: {command} {string.Join(" ", args)}");
            await commandHandler.ExecuteAsync(args, raw, this).ConfigureAwait(false);
        }
    }

    internal static (string command, string[] args) ParseCommand(string raw!!)
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
                _output?.Dispose();
                Commands
                    ?.Where(i => i.Value is IDisposable)
                    .Each(i => ((IDisposable)i.Value).Dispose());
            }

            _disposedValue = true;
        }
    }

    // ~IniPreprocessor()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

