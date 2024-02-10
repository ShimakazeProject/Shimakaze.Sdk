using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shimakaze.Sdk.LanguageServer.Extensions;
internal static class SpanExtensions
{
    public static int GetIndexAfter<T>(this in ReadOnlySpan<T> span, in T value, in int index, in int? @default = default)
        where T : IEquatable<T>?
    {
        int i = span[index..].IndexOf(value);

        return i switch
        {
            -1 when @default.HasValue => @default.Value,
            -1 => -1,
            _ => i + index
        };
    }
}
