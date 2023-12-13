using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

[StackTraceHidden]
internal static class ThrowHelper
{
    [DoesNotReturn]
    public static T ThrowNotSupportToken<T>(this JsonTokenType value)
    {
        throw new NotSupportedException("Not Support", new JsonException($"Unsupported Token \"{value}\""));
    }

    [DoesNotReturn]
    public static TResult ThrowNotSupportValue<TValue, TResult>(this TValue value)
    {
        throw new NotSupportedException("Not Support", new JsonException($"Unsupported Value \"{value}\""));
    }

    public static InvalidCastException CastCsfDocumentException => new($"Cannot Convert as CsfDocument");

    public static void ThrowWhenFalse(this bool value, string message)
    {
        if (!value)
            throw new JsonException(message);
    }

    public static void ThrowWhenFalse(this bool value) => value.ThrowWhenFalse("End of Json");

    public static void ThrowWhenNotToken(this JsonTokenType value, JsonTokenType token)
    {
        if (value != token)
            value.ThrowNotSupportToken<int>();
    }

    public static T ThrowWhenNull<T>([NotNull] this T? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }
}