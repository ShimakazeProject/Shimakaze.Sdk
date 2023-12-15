using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

[StackTraceHidden]
internal static class CsfJsonAsserts
{
    public static void IsNotEndOfStream(bool b)
    {
        if (!b)
            throw new EndOfStreamException();
    }
    
    public static void IsProtocol(int protocol, int value)
    {
        if (value != protocol)
            throw new NotSupportedException($"Cannot Support Protocol {value}, This Converter are only supported Protocol {protocol}");
    }

    public static void IsToken(JsonTokenType token, JsonTokenType value)
    {
        if (value != token)
            throw new FormatException($"Token is \"{value}\", but it should be \"{token}\".");
    }

    public static void IsNotNull<T>([NotNull] T? obj)
    {
        if (obj is null)
            throw new InvalidCastException($"Cannot convert Json to {typeof(T)}");
    }

    public static void PropertyIsNull([MaybeNull] List<CsfValue>? values, string propertyName)
    {
        if (values is not null)
            throw new FormatException($"Cannot have property \"{propertyName}\" and property \"values\" at the same time.");
    }
    public static void PropertyIsNull([MaybeNull] CsfValue? value)
    {
        if (value is not null)
            throw new FormatException($"Cannot have property \"value\" and property \"values\" at the same time.");
    }
    public static void PropertyIsNotNull([NotNull] CsfValue? value)
    {
        if (value is null)
            throw new FormatException($"Property \"value\" cannot be null.");
    }
}
