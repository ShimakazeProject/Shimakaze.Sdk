using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Shimakaze.Sdk.Inilyn.Generators;

/// <summary>
/// 生成编译命令
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class CompilerCommandGenerator : IIncrementalGenerator
{
    const string AttributeName = "Shimakaze.Sdk.Inilyn.Command.CommandAttribute";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context => context.AddSource(
            "CommandAttribute.g.cs",
            """
            using System;

            #nullable enable

            namespace Shimakaze.Sdk.Inilyn.Command;

            [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
            internal sealed class CommandAttribute(string command) : Attribute
            {
                public string Command { get; } = command;
            }
            """));

        context.RegisterImplementationSourceOutput(
            context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                (node, ct) => node.IsKind(SyntaxKind.MethodDeclaration),
                (context, ct) =>
                {
                    IMethodSymbol method = (IMethodSymbol)context.TargetSymbol;
                    var attribute = method.GetAttributes().First(i => i.AttributeClass?.ToDisplayString() is AttributeName);
                    var command = attribute.ConstructorArguments.First().Value?.ToString() ?? string.Empty;
                    var methodMetadataName = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.AddMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
                    return (command, methodMetadataName);
                }).Collect(),
            (context, data) =>
        {
            var list = data.Select(i => $"[\"{i.command}\"] = {i.methodMetadataName},");
            var items = string.Join("\r\n        ", list);

            context.AddSource(
                "ParserContext.g.cs",
                $$"""
                using System.Collections.Generic;

                #nullable enable

                namespace Shimakaze.Sdk.Inilyn;

                partial class ParserContext
                {
                    private readonly Dictionary<string, Delegate> _compilerCommands = new()
                    {
                        {{items}}
                    };
                }
                """);
        });
    }
}
