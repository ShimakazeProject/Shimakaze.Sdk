using System.Text;

using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Types;

/// <summary>
/// 源文件的行列计算：把字符偏移量转换为零基的行号与列号。
/// </summary>
public static class SourceLocator
{
    /// <summary>
    /// 计算指定偏移量所在的行与列。
    /// </summary>
    /// <param name="source">源文件。</param>
    /// <param name="offset">字符偏移量，超出范围时会被截断到 [0, Text.Length]。</param>
    public static (int Line, int Column) GetPosition(SourceFile source, int offset)
    {
        int limit = Math.Clamp(offset, 0, source.Text.Length);
        int line = 0;
        int lineStart = 0;

        for (int i = 0; i < limit; i++)
        {
            char current = source.Text[i];

            // 只统计完整换行之后的偏移：位于 \r 与 \n 之间的偏移仍属于上一行。
            if (current is '\r' && (i + 1 >= source.Text.Length || source.Text[i + 1] is not '\n'))
            {
                line++;
                lineStart = i + 1;
            }
            else if (current is '\n')
            {
                line++;
                lineStart = i + 1;
            }
        }

        return (line, limit - lineStart);
    }
}
