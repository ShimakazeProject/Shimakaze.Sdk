using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shimakaze.Sdk.Utils;
public static class StringExtensions
{
    public static int ToInt32(this string? s, int @default = default) => int.TryParse(s, out int result) ? result : @default;
}
