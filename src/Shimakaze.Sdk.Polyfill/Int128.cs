#if !NET7_0_OR_GREATER
using System.Runtime.InteropServices;

namespace System;

/// <summary>
/// This is not a real Int128! Upgrade to .Net 8 if you want to use it!
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Int128
{
    public readonly int Field1;
    public readonly int Field2;
    public readonly int Field3;
    public readonly int Field4;
}
#endif