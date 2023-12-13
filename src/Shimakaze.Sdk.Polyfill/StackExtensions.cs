#if NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class StackExtensions
{
    public static bool TryPeek<T>(this Stack<T> stack, [NotNullWhen(true)] out T? result)

    {
        if (stack.Count is not 0)
        {
            result = stack.Peek();
            return result is not null;
        }
        else
        {
            result = default;
            return false;
        }
    }

    public static bool TryPop<T>(this Stack<T> stack, [NotNullWhen(true)] out T? result)
    {
        if (stack.Count is not 0)
        {
            result = stack.Pop();
            return result is not null;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
#endif