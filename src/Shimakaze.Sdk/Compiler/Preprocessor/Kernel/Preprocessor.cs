using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Compiler.Preprocessor.Commands;

namespace Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

internal sealed class Preprocessor : IPreprocessor
{
    private readonly ILogger<Preprocessor>? _logger;
    private readonly IPreprocessorVariables _variables;
    private readonly IServiceProvider _serviceProvider;

    public Preprocessor(
        IPreprocessorVariables variables,
        IServiceProvider serviceProvider,
        ILogger<Preprocessor>? logger = null)
    {
        _variables = variables;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private async Task ParseLineAsync(string line, long lineNumber, string filePath, Dictionary<string, IPreprocessorCommand> commands, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!line.TrimStart().StartsWith("#"))
            return;

        int index = line.IndexOf('#');
        string[] arr = line[line.IndexOf('#')..].TrimEnd().Split(' ');
        string command = arr[0][1..];
        string[] args = arr[1..];

        if (!commands.TryGetValue(command, out IPreprocessorCommand? preprocessorCommand))
            throw new NotSupportedException($"""
                The preprocessor cannot resolve {command} preprocessor instructions.
                    at {filePath}:{line},{index}.
                """);

        // if (_serviceProvider.GetRequiredService(type) is not IPreprocessorCommand preprocessorCommand)
        //     throw new InvalidOperationException($"""
        //         The preprocessor cannot continue with the {command} preprocessor instruction
        //         because the implementation of the instruction contains errors.
        //             at {filePath}:{line},{index}.
        //         """);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await preprocessorCommand.ExecuteAsync(args, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"""
                {ex.Message}
                    at {filePath}:{line},{index}.
                """, ex);
        }
    }

    /// <inheritdoc/>
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
        if (_logger is not null)
        {
            foreach (var define in _variables.Defines)
                _logger.LogTrace("Define: " + define);
        }

        long lineNumber = 0;
        while (input.Peek() is not -1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await input.ReadLineAsync();
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
}
