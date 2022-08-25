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
    public override IIniDocument Read(TextReader tr, ReadOptions? options = default)
    {
        Func<bool> check = tr is StreamReader sr ? (() => !sr.EndOfStream) : (() => tr.Peek() >= 0);

        List<IIniLine> buffer = new();
        IIniDocument document = new IniDocument();
        IIniSection currentSection = document.Default;
        uint lineNumber = 0;
        _lines = new();
        while (check())
        {
            var line = tr.ReadLine();
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

    public override void Write(IIniDocument ini, TextWriter tw, IniLoaderWriteOptions? options = default)
    {
        options ??= IniLoaderWriteOptions.Default;

        WriteIniLines(ini.Default, tw, options.IgnoreSummary);

        tw.WriteLine();

        foreach (var section in ini)
        {
            if (section.BeforeSummaries is not null && section.BeforeSummaries.Length > 0)
                section.BeforeSummaries.Each(tw.WriteLine);

            tw.WriteLine($"[{section.Name}]");
            if (string.IsNullOrWhiteSpace(section.Summary))
                tw.WriteLine(section.Summary);

            WriteIniLines(section, tw, options.IgnoreSummary);

            tw.WriteLine();
        }
    }

    private static void WriteIniLines(IEnumerable<IIniLine> lines, TextWriter tw, bool ignoreSummary = false)
    {
        foreach (var line in lines)
            tw.WriteLine(line.ToString(ignoreSummary));
    }
}
