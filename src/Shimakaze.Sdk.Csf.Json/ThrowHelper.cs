using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

internal static class ThrowHelper
{
    public static void ThrowWhenNotToken(this JsonTokenType value, JsonTokenType token)
    {
        if (value != token)
            value.ThrowNotSupportToken<int>();
    }

    public static void ThrowWhenFalse(this bool value, string message)
    {
        if (!value)
            throw new JsonException(message);
    }

    public static T ThrowWhenNull<T>([NotNull] this T? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public static void ThrowWhenFalse(this bool value) => value.ThrowWhenFalse("End of Json");

    [DoesNotReturn]
    public static T ThrowNotSupportToken<T>(this JsonTokenType value)
    {
        throw new JsonException($"Not Support Token \"{value}\"");
    }

    [DoesNotReturn]
    public static TResult ThrowNotSupportValue<TValue, TResult>(this TValue value)
    {
        throw new JsonException($"Not Support Value \"{value}\"");
    }
}