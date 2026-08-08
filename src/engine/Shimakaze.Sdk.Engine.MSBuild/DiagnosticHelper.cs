using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Engine.MSBuild;

public static class DiagnosticHelper
{
    public static void ReportDiagnostic(this TaskLoggingHelper log, Diagnostic diagnostic, bool notError = false)
    {
        Action<string?, string?, string?, string?, int, int, int, int, string> action = diagnostic.Severity switch
        {
            DiagnosticSeverity.Error when !notError => (subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message)
                => log.LogError(subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message),
            DiagnosticSeverity.Warning or DiagnosticSeverity.Error => (subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message)
                => log.LogWarning(subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message),
            DiagnosticSeverity.Info => (subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message)
                => log.LogMessage(subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, MessageImportance.High, message),

            _ => throw new NotSupportedException(),
        };

        action(
            "Inilyn",
            diagnostic.Code,
            diagnostic.Code,
            diagnostic.FilePath,
            diagnostic.Line,
            diagnostic.Column,
            diagnostic.EndLine,
            diagnostic.EndColumn,
            diagnostic.Message);
    }
    public static void ReportDiagnostics(this TaskLoggingHelper log, IEnumerable<Diagnostic> diagnostics, bool notError = false)
    {
        foreach (var diagnostic in diagnostics)
            log.ReportDiagnostic(diagnostic, notError);
    }

}