using System.Globalization;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Document Converter.
/// </summary>
public class CsfDocumentConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return typeof(CsfDocument).IsAssignableFrom(type);
    }

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
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
        List<CsfData> datas = [];

        parser.Consume<MappingStart>();
        while (!parser.TryConsume<MappingEnd>(out _))
        {
            if (parser.TryConsume<Scalar>(out Scalar? scalar))
            {
                switch (scalar.Value)
                {
                    case "lang":
                        if (parser.TryConsume<Scalar>(out Scalar? scalar1))
                        {
                            if (!int.TryParse(scalar1.Value, out int lang))
                            {
                                lang = YamlConstants.LanguageList.IndexOf(scalar1.Value);
                            }

                            metadata.Language = lang;
                        }

                        break;

                    case "version":
                        if (parser.TryConsume<Scalar>(out Scalar? scalar2))
                        {
                            metadata.Version = int.Parse(scalar2.Value, CultureInfo.InvariantCulture);
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
            if (rootDeserializer(typeof(CsfData)) is CsfData data)
            {
                datas.Add(data);
            }
        }

        metadata.Identifier = CsfConstants.CsfFlagRaw;
        metadata.LabelCount = doc.Data.Length;
        metadata.StringCount = doc.Data.Sum(i => i.StringCount);
        doc.Metadata = metadata;
        doc.Data = [.. datas];
        doc.ReCount();
        return doc;
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
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
            emitter.Emit(new Scalar(doc.Metadata.Language.ToString(CultureInfo.InvariantCulture)));
        }

        emitter.Emit(new Scalar("version"));
        emitter.Emit(new Scalar(doc.Metadata.Version.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new MappingEnd());
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart());
        emitter.Emit(new Comment($"yaml-language-server: $schema={YamlConstants.SchemaUrls.V1}", false));
        foreach (CsfData item in doc.Data)
        {
            serializer(item, item.GetType());
        }

        emitter.Emit(new MappingEnd());
    }
}
