using System.Globalization;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// Csf Data Yaml Converter.
/// </summary>
public sealed class CsfDataYamlConverterV1 : YamlConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData? Read(IParser parser, ObjectDeserializer deserializer, YamlSerializerOptions options)
    {
        parser.Consume<DocumentStart>();
        parser.Consume<MappingStart>();

        CsfMetadata metadata = new();

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            var key = parser.Consume<Scalar>();

            switch (key.Value)
            {
                case "lang":
                    string? language = parser.Consume<Scalar>().Value;
                    metadata.Language = int.TryParse(language, out int value) ? value : YamlConstants.LanguageList.IndexOf(language);
                    break;

                case "version":
                    metadata.Version = int.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);
                    break;

                default:
                    parser.SkipThisAndNestedEvents();
                    break;
            }
        }

        parser.Consume<DocumentEnd>();
        parser.Consume<DocumentStart>();

        if (!options.TryGetConverter<CsfValueYamlConverterV1>(out var converter))
            converter = new();

        parser.Consume<MappingStart>();

        List<CsfLabel> labels = [];

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            string? name = parser.Consume<Scalar>().Value;
            var value = converter.Read(parser, deserializer, options);

            labels.Add(new(name, value is null ? [] : [value]));
        }

        var result = new CsfData(metadata, labels);
        result.UpdateMetadataCount();

        return result;
    }

    /// <inheritdoc/>
    public override void Write(IEmitter emitter, CsfData value, ObjectSerializer serializer, YamlSerializerOptions options)
    {
        emitter.Emit(new MappingStart());
        emitter.Emit(new Comment($"yaml-language-server: $schema={YamlConstants.SchemaUrls.V1.Head}", false));

        emitter.Emit(new Scalar("lang"));

        if (value.Metadata.Language.Value >= 0 && value.Metadata.Language.Value < YamlConstants.LanguageList.Count)
            emitter.Emit(new Scalar(YamlConstants.LanguageList[value.Metadata.Language.Value]));
        else
            emitter.Emit(new Scalar(value.Metadata.Language.Value.ToString(CultureInfo.InvariantCulture)));

        emitter.Emit(new Scalar("version"));
        emitter.Emit(new Scalar(value.Metadata.Version.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new MappingEnd());

        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new DocumentStart());

        emitter.Emit(new Comment($"yaml-language-server: $schema={YamlConstants.SchemaUrls.V1.Data}", false));
        emitter.Emit(new MappingStart());

        foreach (var label in value.Labels)
        {
            emitter.Emit(new Scalar(label.Name));

            switch (label.Count)
            {
                case 0:
                    emitter.Emit(new Scalar("null"));
                    break;

                case 1:
                    serializer(label[0], typeof(CsfValue));
                    break;

                default:
                    emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));

                    foreach (var item in label)
                        serializer(item, typeof(CsfValue));

                    emitter.Emit(new SequenceEnd());
                    break;
            }
        }

        emitter.Emit(new MappingEnd());
    }
}
