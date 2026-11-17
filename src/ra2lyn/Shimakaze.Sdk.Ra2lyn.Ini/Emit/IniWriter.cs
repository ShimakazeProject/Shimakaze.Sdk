using System.Text;

using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Emit;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Emit;

/// <summary>
/// INI 发射器：把 <see cref="CompilationModel"/> 按组/文档顺序写回 INI 文本。
/// </summary>
/// <remarks>
/// <para>
/// 输出语法固定为：<c>[Name]</c>、可选的继承 <c>:[Base1],[Base2]</c>、随后每行一个 <c>Key=Value</c>；
/// 每节之后输出一个空行（即两节之间恰好一个空行）。
/// </para>
/// <para>
/// 多文档：按实体所属规则组分文件，文件名为 <c>{组名}.ini</c>（组名为空时使用 <c>output.ini</c>）。
/// </para>
/// <para>
/// 注释不保留；MVP 不支持 <see cref="EmitOptions.PreserveComments"/>，传入 <see langword="true"/> 时抛
/// <see cref="NotSupportedException"/>，禁止静默忽略。
/// </para>
/// </remarks>
public sealed class IniWriter : IEmitter
{
    /// <inheritdoc/>
    public EmitResult Emit(CompilationModel model, EmitOptions options)
    {
        if (options.PreserveComments)
            throw new NotSupportedException("INI 发射器暂不支持保留注释");

        string newLine = options.NewLine;
        Dictionary<string, StringBuilder> files = new(StringComparer.OrdinalIgnoreCase);

        // TreeShaking 只依据分析阶段给出的可达集合；没有可达集合时不做裁剪。
        HashSet<EntityId>? reachable = options.TreeShaking && model.ReachableEntities is { } set
            ? [.. set]
            : null;

        foreach (var entity in model.Entities)
        {
            if (reachable is not null && reachable.Contains(entity.Id) is false)
                continue;

            string fileName = GetFileName(entity.Group);
            if (files.TryGetValue(fileName, out var builder) is false)
            {
                builder = new();
                files[fileName] = builder;
            }

            WriteEntity(builder, entity, newLine);
        }

        // 输出的键顺序即首次写出顺序：先按实体所属组登记，再按字典的稳定插入顺序导出。
        Dictionary<string, string> output = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string fileName, var builder) in files)
            output[fileName] = builder.ToString();


        return new EmitResult(output, []);
    }

    /// <summary>
    /// 按规则组取输出文件名；未提供组名时使用固定文件名。
    /// </summary>
    private static string GetFileName(string group)
        => string.IsNullOrEmpty(group) ? "output.ini" : $"{group}.ini";

    /// <summary>
    /// 写出一个实体：节头、继承与键值，末尾补一个空行。
    /// </summary>
    private static void WriteEntity(StringBuilder builder, EntityModel entity, string newLine)
    {
        builder.Append('[').Append(entity.Name).Append(']');

        if (entity.BaseNames.Count is not 0)
        {
            builder.Append(':');
            for (int index = 0; index < entity.BaseNames.Count; index++)
            {
                if (index is not 0)
                    builder.Append(',');

                builder.Append('[').Append(entity.BaseNames[index]).Append(']');
            }
        }

        builder.Append(newLine);

        foreach (var property in entity.Properties)
            builder.Append(property.Key).Append('=').Append(property.Value).Append(newLine);

        builder.Append(newLine);
    }
}