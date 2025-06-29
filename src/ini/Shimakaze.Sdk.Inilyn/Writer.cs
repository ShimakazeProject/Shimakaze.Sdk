using Shimakaze.Sdk.Inilyn.Models.Emit;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// Ini文档写入器
/// </summary>
public static class Writer
{
    /// <summary>
    /// 将Ini文档写入流
    /// </summary>
    /// <param name="document"></param>
    /// <param name="writer"></param>
    /// <param name="options"></param>
    public static void WriteTo(this IniDocument document, TextWriter writer, IniWriterOptions? options = null)
    {
        options ??= IniWriterOptions.Default;

        Action<KeyValuePair<string, string>> kvpWriter = options switch
        {
            { SpaceBeforeEquals: true, SpaceAfterEquals: true } => kvp => writer.WriteLine($"{kvp.Key} = {kvp.Value}"),
            { SpaceBeforeEquals: true, SpaceAfterEquals: false } => kvp => writer.WriteLine($"{kvp.Key} ={kvp.Value}"),
            { SpaceBeforeEquals: false, SpaceAfterEquals: true } => kvp => writer.WriteLine($"{kvp.Key}= {kvp.Value}"),
            { SpaceBeforeEquals: false, SpaceAfterEquals: false } => kvp => writer.WriteLine($"{kvp.Key}={kvp.Value}"),
        };

        foreach (var section in document.Sections)
        {
            if (section.Name is not Binder.DefaultSectionName)
                writer.WriteLine($"[{section.Name}]");

            foreach (var kvp in section)
                kvpWriter(kvp);
        }
    }
}
