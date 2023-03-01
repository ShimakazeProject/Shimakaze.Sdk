using Shimakaze.Sdk.Data.Csf;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Text.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Document Converter.
/// </summary>
public class CsfDocumentConverter : IYamlTypeConverter
{
    private static readonly WeakReference<CsfDocumentConverter> WeakReference = new(new());

    /// <summary>
    /// Gets csfValueConverter.
    /// </summary>
    public static CsfDocumentConverter Instance
    {
        get
        {
            if (!WeakReference.TryGetTarget(out CsfDocumentConverter? converter))
            {
                WeakReference.SetTarget(converter = new());
            }

            return converter;
        }
    }

    /// <inheritdoc/>
    public bool Accepts(Type type) => typeof(CsfDocument).IsAssignableFrom(type);

    /// <inheritdoc/>
    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is not MappingStart mappingStart)
        {
            if (parser.Current is null)
            {
                throw new FormatException("???");
            }

            Mark start = parser.Current.Start;
            Mark end = parser.Current.End;
            throw new YamlException(start, end, "Unknown Token");
        }

        CsfDocument doc = new();
        CsfMetadata metadata = doc.Metadata;

        parser.Consume<MappingStart>();
        while (!parser.TryConsume<MappingEnd>(out _))
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                switch (scalar.Value)
                {
                    case "lang":
                        if (parser.TryConsume<Scalar>(out var scalar1))
                        {
                            if (!int.TryParse(scalar1.Value, out int lang))
                            {
                                lang = YamlConstants.LanguageList.IndexOf(scalar1.Value);
                            }

                            metadata.Language = lang;
                        }

                        break;
                    case "version":
                        if (parser.TryConsume<Scalar>(out var scalar2))
                        {
                            metadata.Version = int.Parse(scalar2.Value);
                        }

                        break;
                }
            }
        }

        parser.TryConsume<DocumentEnd>(out _);
        parser.TryConsume<DocumentStart>(out _);
        parser.Consume<MappingStart>();

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            if (CsfDataConverter.Instance.ReadYaml(parser, typeof(CsfData)) is CsfData data)
            {
                doc.Add(data);
            }
        }

        metadata.Identifier = CsfConstants.CsfFlagRaw;
        metadata.LabelCount = doc.Data.Count;
        metadata.StringCount = doc.Data.Sum(i => i.StringCount);
        doc.Metadata = metadata;
        return doc;
    }

    /// <inheritdoc/>
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is not CsfDocument doc)
        {
            return;
        }

        emitter.Emit(new MappingStart());
        emitter.Emit(new Comment($"yaml-language-server: $schema={YamlConstants.SchemaUrls.Metadata}", false));
        emitter.Emit(new Scalar("lang"));
        if (doc.Metadata.Language < YamlConstants.LanguageList.Count)
        {
            emitter.Emit(new Scalar(YamlConstants.LanguageList[doc.Metadata.Language]));
        }
        else
        {
            emitter.Emit(new Scalar(doc.Metadata.Language.ToString()));
        }

        emitter.Emit(new Scalar("version"));
        emitter.Emit(new Scalar(doc.Metadata.Version.ToString()));
        emitter.Emit(new MappingEnd());
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart());
        emitter.Emit(new Comment($"yaml-language-server: $schema={YamlConstants.SchemaUrls.V1}", false));
        foreach (CsfData item in doc.Data)
        {
            CsfDataConverter.Instance.WriteYaml(emitter, item, item.GetType());
        }

        emitter.Emit(new MappingEnd());
    }
}
