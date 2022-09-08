using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Models.Tmp;

[StructLayout(LayoutKind.Sequential)]
public record struct Color
{
    public byte Red;
    public byte Green;
    public byte Blue;
}