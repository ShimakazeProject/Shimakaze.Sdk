using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Tmp;

/// <summary>
/// RGB Color
/// </summary>
/// <param name="R"></param>
/// <param name="G"></param>
/// <param name="B"></param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct RGBColor(byte R, byte G, byte B);
