using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

[StackTraceHidden]
internal static class CsfJsonAsserts
{
    public static void IsKind(this in JsonElement json, in JsonValueKind kind)
    {
        if (json.ValueKind != kind)
            throw new FormatException($"Json element is \"{json.ValueKind}\", but it should be \"{kind}\".");
    }

    public static void HasProtocol(this in JsonElement json, in int protocol)
    {
        if (!json.TryGetProperty("protocol", out var value)
            || protocol != value.GetInt32())
            throw new NotSupportedException($"Cannot Support Protocol {value}, This Converter are only supported Protocol {protocol}");
    }
}
