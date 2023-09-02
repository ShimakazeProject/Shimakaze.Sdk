using System.Collections.Immutable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器引擎设置
/// </summary>
public sealed class EngineOptions
{
    /// <summary>
    /// 命令集
    /// </summary>
    public ImmutableArray<Command> Commands { get; set; } = new();

    /// <summary>
    /// 定义集
    /// </summary>
    public HashSet<string> Defines { get; set; } = new();
}

/// <summary>
/// 预处理器引擎
/// </summary>
public sealed class Engine
{
    private readonly ImmutableArray<Command> _commands;
    private readonly Dictionary<string, object> _storage;
    private readonly IServiceProvider _provider;
    private readonly ILogger<Engine>? _logger;
    /// <summary>
    /// 是否可写入到输出流
    /// </summary>
    public bool CanWritable { get; set; }

    /// <summary>
    /// 预处理器引擎
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public Engine(IServiceProvider provider, EngineOptions options, ILogger<Engine>? logger = null)
    {
        _provider = provider;
        _logger = logger;
        _commands = options.Commands;
        _storage = new()
        {
            ["Defines"] = options.Defines
        };
        _logger?.LogTrace("Define: {define}", string.Join(';', options.Defines));
    }

    /// <summary>
    /// 获取或创建新的变量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="creator"></param>
    /// <returns></returns>
    public T GetOrNew<T>(string key, Func<T> creator) where T : notnull
    {
        if (_storage.TryGetValue(key, out object? value) && value is T result)
            return result;
        result = creator();
        _storage.Add(key, result);
        return result;
    }

    private void ParseLine(string line, long lineNumber, string? filePath = default)
    {
        int index = line.IndexOf('#');

        var cmd = _commands.FirstOrDefault(i => i.CanExecute(line.Trim()))
            ?? throw new NotSupportedException($"""
            Unknown Preprocessor Command {line.Trim()}
                at {filePath}:{lineNumber},{index}
            """);

        try
        {
            cmd.Execute(_provider.GetRequiredService(cmd.Type), line);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"""
                {ex.Message}
                    at {filePath}:{lineNumber},{index}.
                """, ex);
        }
    }

    /// <summary>
    /// 执行预处理器
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public void Execute(
        TextReader input,
        TextWriter output,
        string? filePath = default)
    {

        long lineNumber = 0;
        while (input.Peek() is not -1)
        {
            string? line = input.ReadLine();
            lineNumber++;

            if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith('#'))
            {
                ParseLine(line, lineNumber, filePath);
                continue;
            }

            if (CanWritable)
                output.WriteLine(line);
        }
        output.Flush();
    }
}
