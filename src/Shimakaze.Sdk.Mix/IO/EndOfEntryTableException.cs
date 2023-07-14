﻿using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Entry Table已结束
/// </summary>
[Serializable, ExcludeFromCodeCoverage]
public class EndOfEntryTableException : Exception
{
    /// <inheritdoc />
    public EndOfEntryTableException()
    { }

    /// <inheritdoc />
    public EndOfEntryTableException(string message) : base(message) { }

    /// <inheritdoc />
    public EndOfEntryTableException(string message, Exception inner) : base(message, inner) { }

    /// <inheritdoc />
    protected EndOfEntryTableException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}