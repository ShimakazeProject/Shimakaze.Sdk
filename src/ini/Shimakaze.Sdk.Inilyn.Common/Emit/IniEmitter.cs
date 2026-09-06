using Microsoft.EntityFrameworkCore;

using Shimakaze.Sdk.Inilyn.Data;
using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Emit;

public sealed class IniEmitter(IniDbContext db)
{
    public async Task EmitAsync(string outputPath, bool treeShaking = false, CancellationToken cancellationToken = default)
    {
        await using var writer = File.CreateText(outputPath);
        await EmitAsyncCore(writer, treeShaking, cancellationToken);
    }

    public Task EmitAsync(StreamWriter writer, bool treeShaking = false, CancellationToken cancellationToken = default)
        => EmitAsyncCore(writer, treeShaking, cancellationToken);

    public async Task<string> EmitToStringAsync(bool treeShaking = false, CancellationToken cancellationToken = default)
    {
        await using StringWriter writer = new();
        await EmitAsyncCore(writer, treeShaking, cancellationToken);
        return writer.ToString();
    }

    private async Task EmitAsyncCore(TextWriter writer, bool treeShaking, CancellationToken cancellationToken)
    {
        var sections = treeShaking
            ? await db.Sections
                .Include(s => s.KeyValues)
                .Include(s => s.Inheritances)
                .LeftJoin(db.SectionSemantics,
                    section => section.Id,
                    semantic => semantic.SectionId,
                    (section, semantic) => new { Section = section, Semantic = semantic })
                .Where(i => i.Semantic != null && i.Semantic.IsReachable)
                .OrderBy(s => s.Section.DocumentId)
                .ThenBy(s => s.Section.Order)
                .Select(s => s.Section)
                .ToListAsync(cancellationToken)
            : await db.Sections
                .Include(s => s.KeyValues)
                .Include(s => s.Inheritances)
                .OrderBy(s => s.DocumentId)
                .ThenBy(s => s.Order)
                .ToListAsync(cancellationToken);

        foreach (var section in sections)
        {
            await EmitSectionAsync(writer, section, cancellationToken);
            await writer.WriteLineAsync(cancellationToken);
        }
    }

    private static async Task EmitSectionAsync(TextWriter writer, SectionNode section, CancellationToken cancellationToken)
    {
        await writer.WriteAsync('[');
        await writer.WriteAsync(section.Name, cancellationToken);
        await writer.WriteAsync(']');

        if (section is { Inheritances.Count: not 0 })
        {
            bool first = true;
            foreach (var inheritance in section.Inheritances.OrderBy(i => i.Order))
            {
                await writer.WriteAsync(first ? ':' : ',');
                await writer.WriteAsync(inheritance.Name, cancellationToken);
                first = false;
            }
        }

        await writer.WriteLineAsync(cancellationToken);

        foreach (var kv in section.KeyValues.OrderBy(k => k.Order))
            await EmitKeyValueAsync(writer, kv, cancellationToken);
    }

    private static async Task EmitKeyValueAsync(TextWriter writer, KeyValuePairNode kv, CancellationToken cancellationToken)
    {
        await writer.WriteAsync(kv.Key, cancellationToken);
        await writer.WriteAsync('=');
        await writer.WriteAsync(kv.Value, cancellationToken);

        await writer.WriteLineAsync(cancellationToken);
    }
}
