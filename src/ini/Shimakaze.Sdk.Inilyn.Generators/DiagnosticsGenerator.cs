using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;

namespace Shimakaze.Sdk.Inilyn.Generators;

/// <summary>
/// Diagnostics 生成器
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DiagnosticsGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Regex regex = new(@"\{(.+?)\}");

        context.RegisterImplementationSourceOutput(
            context.AdditionalTextsProvider.Where(i => Path.GetFileName(i.Path) is "Diagnostics.csv"),
            (context, file) =>
            {
                if (file.GetText() is not { } text)
                    return;

                var data = text.Lines
                    .Skip(1)
                    .Select(i => i.ToString().Split(','))
                    .Where(i => i is { Length: >= 2 })
                    .Select(i => new
                    {
                        Code = i[0],
                        Severity = i[1] switch
                        {
                            "Err" => "Error",
                            "Warn" => "Warning",
                            "Info" => "Information",
                            _ => i[1],
                        },
                        Summary = i.Length > 2 ? i[2].Trim('"', '\'') : string.Empty,
                    })
                    .Select(i =>
                    {
                        var matches = regex.Matches(i.Summary);
                        StringBuilder sb = new();
                        StringBuilder sb2 = new();
                        StringBuilder sb3 = new();
                        foreach (var item in matches.OfType<Match>())
                        {
                            sb.Append("object? ")
                                .Append(item.Groups[1].Value)
                                .Append(", ");
                            sb2.Append(item.Groups[1].Value)
                                .Append(", ");
                            sb3.Append("object?, ");
                        }

                        var code = i.Code[0] is >= '0' and <= '9' ? $"INI{i.Code}" : i.Code;
                        return $$"""
                        /// <summary>
                        /// {{i.Summary}}
                        /// </summary>
                        /// <param name="range">范围</param>
                        /// <returns>诊断信息</returns>
                        public static Diagnostic {{code}}({{sb}}Draco.Lsp.Model.Range range) => new()
                        {
                            Code = "{{code}}",
                            Severity = DiagnosticSeverity.{{i.Severity}},
                            Message = $"{{i.Summary.Replace("\"", "\\\"")}}",
                            Range = range,
                        };
                        /// <inheritdoc cref="{{code}}({{sb3}}Draco.Lsp.Model.Range)"/>
                        /// <param name="line">行号</param>
                        /// <param name="column">列号</param>
                        /// <param name="length">长度</param>
                        public static Diagnostic {{code}}({{sb}}int line, int column, int length) => {{code}}({{sb2}}new()
                        {
                            Start = new()
                            {
                                Line = unchecked((uint)(line)),
                                Character = unchecked((uint)(column)),
                            },
                            End = new()
                            {
                                Line = unchecked((uint)(line)),
                                Character = unchecked((uint)(column + length)),
                            },
                        });

                        """;
                    });

                context.AddSource(
                    "Diagnostics.g.cs",
                    $$"""
                    using Draco.Lsp.Model;

                    #nullable enable

                    namespace Shimakaze.Sdk.Inilyn;
                                        
                    #pragma warning disable 1573
                    internal static class Diagnostics
                    {
                    {{string.Join("\r\n", data)}}
                    }
                    """);
            });
    }
}
