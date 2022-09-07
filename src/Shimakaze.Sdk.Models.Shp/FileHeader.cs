using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Models.Shp;



[StructLayout(LayoutKind.Sequential)]
public struct FileHeader
{
    public ushort Reserved;
    public ushort Width;
    public ushort Height;
    public ushort FrameCount;
}
