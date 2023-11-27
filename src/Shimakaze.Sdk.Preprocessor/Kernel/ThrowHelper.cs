using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

[StackTraceHidden]
internal static class ThrowHelper
{
    private static string CreateExceptionMessage(this Engine engine, string message, int? col = default)
    {
        StringBuilder sb = new(message);

        if (!string.IsNullOrEmpty(engine.FilePath))
        {
            sb.AppendLine()
                .Append("    at ")
                .Append(engine.FilePath)
                .Append(':')
                .Append(engine.Line);
            if (col.HasValue)
                sb.Append(',')
                    .Append(col.Value);

            sb.Append('.');
        }

        return sb.ToString();
    }

    [DoesNotReturn]
    public static T ThrowNotSupport<T>(this Engine engine, string? message = default, int? col = default) => throw new NotSupportedException(
        engine.CreateExceptionMessage(message ?? "We do not support this operation."));

    [DoesNotReturn]
    public static void Throw(this Engine engine, Exception ex, int? col = default) => throw new PreprocessorException(
        engine.CreateExceptionMessage("Program terminated due to unknown error."),
        ex);

}