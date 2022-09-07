using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Models.Common;

[StructLayout(LayoutKind.Sequential)]
public struct RGB
{
    public byte Red;
    public byte Green;
    public byte Blue;
}