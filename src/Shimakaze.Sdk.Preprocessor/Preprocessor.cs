using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor;

internal sealed class Preprocessor : IPreprocessor
{
    private readonly ILogger<Preprocessor>? _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPreprocessorVariables _variables;

    public Preprocessor(
        IPreprocessorVariables variables,
        IServiceProvider serviceProvider,
        ILogger<Preprocessor>? logger = null)
    {
        _variables = variables;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(
        TextReader input,
        TextWriter output,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, IPreprocessorCommand> commands = _serviceProvider
            .GetServices<IPreprocessorCommand>()
            .ToDictionary(i => i
                .GetType()
                .GetCustomAttribute<PreprocessorCommandAttribute>()
                ?.CommandName
                ?? throw new InvalidOperationException(
                    $"Type {i.GetType()} does not have the PreprocessorCommandAttribute attribute."
                ));

        cancellationToken.ThrowIfCancellationRequested();
        _logger?.LogTrace("Define: {define}", string.Join(';', _variables.Defines));

        long lineNumber = 0;
        while (input.Peek() is not -1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await input.ReadLineAsync(cancellationToken);
            lineNumber++;

            if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith('#'))
            {
                await ParseLineAsync(line, lineNumber, filePath, commands, cancellationToken);
                continue;
            }

            if (_variables.WriteOutput)
                await output.WriteLineAsync(line);
        }
        await output.FlushAsync();
    }

    private static async Task ParseLineAsync(string line, long lineNumber, string filePath, Dictionary<string, IPreprocessorCommand> commands, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int index = line.IndexOf('#');
        string[] arr = line[line.IndexOf('#')..].TrimEnd().Split(' ');
        string command = arr[0][1..];
        string[] args = arr[1..];

        if (!commands.TryGetValue(command, out IPreprocessorCommand? preprocessorCommand))
            throw new NotSupportedException($"""
                The preprocessor cannot resolve {command} preprocessor instructions.
                    at {filePath}:{lineNumber},{index}.
                """);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await preprocessorCommand.ExecuteAsync(args, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"""
                {ex.Message}
                    at {filePath}:{lineNumber},{index}.
                """, ex);
        }
    }
}