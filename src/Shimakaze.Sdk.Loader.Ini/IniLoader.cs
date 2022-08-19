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
    public override IIniDocument Read(TextReader tr, ReadOptions? options)
    {
        Func<bool> check = tr is StreamReader sr ? (() => !sr.EndOfStream) : (() => tr.Peek() >= 0);

        List<string> buffer = new();
        IIniDocument document = new IniDocument();
        IIniSection currentSection = document.Default;
        uint lineNumber = 0;
        try
        {
            while (check())
            {
                var line = tr.ReadLine();
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (buffer.Count > 0)
                        buffer.ForEach(i => currentSection.Add(new IniLine(i)));

                    continue;
                }

                if (line.StartsWith("["))
                {
                    var iStart = line.IndexOf('[') + 1;
                    var iEnd = line.IndexOf(']') - 2;
                    var iSummary = line.IndexOf(';') + 1;
                    if (iSummary >= 0 && iEnd > iSummary)
                    {
                        throw new FormatException($"The ']' cannot after the ';' at line:{lineNumber}. \n\t{line}");
                    }
                    var section = line[iStart..iEnd];
                    document.Add(currentSection = new IniSection(section));

                    if (buffer.Count > 0)
                        currentSection.BeforeSummaries = buffer.ToArray();

                    continue;
                }

                IIniLine kvp = new IniLine(line);
                if (kvp.IsEmptyKey)
                {
                    buffer.Add(line);
                    continue;
                }

                currentSection.Add(kvp);
            }
        }
        catch (Exception e) when (e is not FormatException)
        {
            throw new InvalidProgramException($"Unknown Exception at line:{lineNumber}.", e);
        }

        return document;
    }

    public override void Write(IIniDocument ini, TextWriter tw, IniLoaderWriteOptions? options)
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
