using System.Text;

using Shimakaze.Sdk.Models.Ini;
using Shimakaze.Sdk.Models.Ini.implements;
using Shimakaze.Sdk.Preprocessor.Ini;
using Shimakaze.Sdk.Utils;

namespace Shimakaze.Sdk.Loader.Ini;

public sealed class IniLoaderWriteOptions : WriteOptions<IniLoaderWriteOptions>
{
    public bool IgnoreSummary { get; init; } = false;

}

public sealed class IniLoader : TextLoader<IIniDocument, ReadOptions, IniLoaderWriteOptions>
{
    private Dictionary<uint, object?>? _lines;

    public Dictionary<uint, object?> Lines => _lines!;
    public override async Task<IIniDocument> ReadAsync(TextReader tr, ReadOptions? options = default)
    {
        Func<bool> check = tr is StreamReader sr ? (() => !sr.EndOfStream) : (() => tr.Peek() >= 0);

        List<IIniLine> buffer = new();
        IIniDocument document = new IniDocument();
        IIniSection currentSection = document.Default;
        uint lineNumber = 0;
        _lines = new();
        while (check())
        {
            var line = await tr.ReadLineAsync();
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                if (buffer.Count > 0)
                {
                    buffer.ForEach(currentSection.Add);
                    buffer.Clear();
                }

                continue;
            }

            if (line.StartsWith("["))
            {
                var iStart = line.IndexOf('[') + 1;
                var iEnd = line.IndexOf(']');
                var iSummary = line.IndexOf(';') + 1;
                if (iSummary - 1 >= 0 && iEnd > iSummary)
                {
                    throw new FormatException($"The ']' cannot after the ';' at line:{lineNumber}. \n\t{line}");
                }
                var section = line[iStart..iEnd];
                document.Add(currentSection = new IniSection(section));

                if (buffer.Count > 0)
                {
                    currentSection.BeforeSummaries = buffer.ToArray();
                    buffer.Clear();
                }

                Lines.Add(lineNumber, currentSection);
                continue;
            }

            IIniLine kvp = new IniLine(line, lineNumber);
            Lines.Add(lineNumber, kvp);
            if (kvp.IsEmptyKey)
            {
                buffer.Add(kvp);
                continue;
            }

            currentSection.Add(kvp);
        }

        if (buffer.Count > 0)
            buffer.ForEach(currentSection.Add);


        return document;
    }

    public override async Task WriteAsync(IIniDocument ini, TextWriter tw, IniLoaderWriteOptions? options = default)
    {
        options ??= IniLoaderWriteOptions.Default;

        await WriteIniLines(ini.Default, tw, options.IgnoreSummary);

        await tw.WriteLineAsync();

        foreach (var section in ini)
        {
            if (!options.IgnoreSummary
                && section.BeforeSummaries is not null
                && section.BeforeSummaries.Length > 0)
                await WriteIniLines(section.BeforeSummaries, tw);

            await tw.WriteLineAsync($"[{section.Name}]");

            if (!options.IgnoreSummary
                && string.IsNullOrWhiteSpace(section.Summary))
                await tw.WriteLineAsync(section.Summary);

            await WriteIniLines(section, tw, options.IgnoreSummary);

            await tw.WriteLineAsync();
        }
    }

    private static async ValueTask WriteIniLines(IEnumerable<IIniLine> lines, TextWriter tw, bool ignoreSummary = false)
    {
        foreach (var line in lines)
            await tw.WriteLineAsync(line.ToString(ignoreSummary));
    }
}
