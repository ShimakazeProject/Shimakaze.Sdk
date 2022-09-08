using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shimakaze.Sdk.Utils;
public static class UnsafeExtension
{
    public static IntPtr CopyTo(this byte[] bytes, ref IntPtr ptr, int startIndex, int length)
    {
        Marshal.Copy(bytes, startIndex, ptr, length);
        return ptr;
    }
}
