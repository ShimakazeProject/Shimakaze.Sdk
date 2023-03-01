using Shimakaze.Sdk.Text.Serialization;

namespace Shimakaze.Sdk.Text.Ini.Serialization;

// It's not support static abstract method.
#pragma warning disable SA1648 // inheritdoc should be used with inheriting class

/// <summary>
/// An Ini Serializer.
/// </summary>
public sealed class IniSerializer : ITextSerializer<IniDocument, IniSerializerOptions>
{
    /// <inheritdoc/>
    public static IniDocument Deserialize(TextReader reader, IniSerializerOptions? options = default)
    {
        IniReader sr = new IniReader(reader);
        IniDocument ini = new();
        IniSection section = ini.Default;
        IniKeyValuePair? current = null;
        bool isSectionComment = false;

        while (sr.Read())
        {
            switch (sr.Token)
            {
                case IniToken.SectionHeader:
                    current = null;
                    section = new(sr.Value);
                    ini.Add(section);
                    isSectionComment = true;
                    break;
                case IniToken.Key:
                    current = new(sr.Value, default, default);
                    section.Add(current);
                    isSectionComment = false;
                    break;
                case IniToken.Value:
                    if (current is null)
                    {
                        throw new FormatException();
                    }

                    current.Value = sr.Value;
                    isSectionComment = false;
                    break;
                case IniToken.Comment:
                    if (isSectionComment)
                    {
                        section.Comment = sr.Value;
                    }
                    else if (current is not null)
                    {
                        current.Comment = sr.Value;
                    }
                    else
                    {
                        section.Add(new IniKeyValuePair(default, default, sr.Value));
                    }

                    isSectionComment = false;
                    current = null;
                    break;
                case IniToken.EmptyLine:
                    isSectionComment = false;
                    current = null;
                    break;
                case IniToken.PreProcessorCommand:
                    // 不处理预处理器
                    continue;
            }
        }

        return ini;
    }

    /// <inheritdoc />
    public static IniDocument Deserialize(Stream stream, IniSerializerOptions? options = default) => Deserialize(new StreamReader(stream));

    /// <inheritdoc />
    public static void Serialize(TextWriter writer, IniDocument document, IniSerializerOptions? options = default)
    {
        IniWriter sw = new(writer, options);
        WriteIniLines(document.Default, sw);

        foreach (var section in document)
        {
            if (section.BeforeComments is not null)
            {
                WriteIniLines(section.BeforeComments, sw);
            }

            sw.Write(IniToken.SectionHeader, section.Name);

            if (!string.IsNullOrWhiteSpace(section.Comment))
            {
                sw.Write(IniToken.Comment, section.Comment);
            }

            sw.Write(IniToken.EmptyLine, string.Empty);

            WriteIniLines(section, sw);
        }
    }

    /// <inheritdoc />
    public static void Serialize(Stream stream, IniDocument document, IniSerializerOptions? options = default) => Serialize(new StreamWriter(stream), document);

    private static void WriteIniLines(IEnumerable<IniKeyValuePair> lines, IniWriter sw)
    {
        foreach (var item in lines)
        {
            if (!string.IsNullOrEmpty(item.Key))
            {
                sw.Write(IniToken.Key, item.Key);
            }

            if (!string.IsNullOrEmpty(item.Value))
            {
                sw.Write(IniToken.Value, item.Value);
            }

            if (!string.IsNullOrEmpty(item.Comment))
            {
                sw.Write(IniToken.Comment, item.Comment);
            }

            sw.Write(IniToken.EmptyLine, string.Empty);
        }
    }
}
