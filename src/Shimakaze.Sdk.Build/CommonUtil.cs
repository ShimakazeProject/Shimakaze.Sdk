using System.Diagnostics.CodeAnalysis;

using Microsoft.Build.Utilities;

namespace Shimakaze.Sdk.Build;

internal static class CommonUtil
{
    /// <summary>
    /// 根据文件路径创建上级目录
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="log">日志帮助程序</param>
    /// <returns></returns>
    public static bool CreateParentDirectory([NotNullWhen(true)] this string? file, TaskLoggingHelper? log = null)
    {
        if (string.IsNullOrEmpty(file))
        {
            log?.LogError($"File path cannot be empty.");
            return false;
        }
        var outdir = Path.GetDirectoryName(file);
        if (string.IsNullOrEmpty(outdir))
        {
            log?.LogError($"Cannot found the parent path from path \"{file}\".");
            return false;
        }
        if (!Directory.Exists(outdir) && !Directory.CreateDirectory(outdir).Exists)
        {
            log?.LogError($"Cannot create the directory at \"{outdir}\".");
            return false;
        }

        return true;
    }
}