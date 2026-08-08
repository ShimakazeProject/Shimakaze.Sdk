using Shimakaze.Sdk.Inilyn.Semantic;
using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn.CodeGeneration;

/// <summary>
/// INI 代码生成器。
/// </summary>
/// <remarks>
/// 遍历展平后的语义模型（Mixin 已展开、TreeShaking 已执行），
/// 输出紧凑的标准 INI 格式文本。
/// </remarks>
public sealed class IniCodeGenerator
{
    /// <summary>
    /// 从语义模型生成标准 INI 文本。
    /// </summary>
    /// <param name="model">展平后的语义模型。</param>
    /// <returns>生成的 INI 源文本。</returns>
    public static SourceText Generate(IniSemanticModel model)
    {
        return Generate(model, fileName: null);
    }

    /// <summary>
    /// 从语义模型生成标准 INI 文本。
    /// </summary>
    /// <param name="model">展平后的语义模型。</param>
    /// <param name="fileName">输出文件名（用于全局键值对的节名）。</param>
    /// <returns>生成的 INI 源文本。</returns>
    public static SourceText Generate(IniSemanticModel model, string? fileName)
    {
        IniEmitter emitter = new();

        // 输出所有节
        foreach (var section in model.Sections)
        {
            emitter.WriteSectionHeader(section.Name);

            foreach (var kv in section.KeyValues)
            {
                emitter.WriteKeyValue(kv.Key, kv.Value);
            }
        }

        // 全局键值对归入以文件名为节名的节下
        if (model.GlobalKeys.Count > 0)
        {
            string sectionName = fileName ?? string.Empty;
            emitter.WriteSectionHeader(sectionName);

            foreach (var kv in model.GlobalKeys)
            {
                emitter.WriteKeyValue(kv.Key, kv.Value);
            }
        }

        return SourceText.Create(emitter.ToString(), fileName);
    }
}
