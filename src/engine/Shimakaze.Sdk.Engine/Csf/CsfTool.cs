using System.Text.Json;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;

namespace Shimakaze.Sdk.Engine.Csf;

/// <summary>
/// Provides utility methods for loading, saving, and format detection of CSF string table data.
/// <br />
/// Supports native CSF, YAML, JSON (v1/v2), and XML formats.
/// </summary>
public static class CsfTool
{
    /// <summary>
    /// Loads <see cref="CsfData"/> from a native CSF binary stream.
    /// </summary>
    /// <param name="input">The stream containing the native CSF binary data.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    public static CsfData LoadFromCsf(Stream input)
        => CsfReader.ReadAllData(input);

    /// <summary>
    /// Loads <see cref="CsfData"/> from a YAML stream.
    /// </summary>
    /// <param name="input">The stream containing the YAML data.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    public static CsfData LoadFromYaml(Stream input)
        => CsfYamlV1Reader.Read(input);

    /// <summary>
    /// Loads <see cref="CsfData"/> from a JSON v2 stream.
    /// </summary>
    /// <param name="input">The stream containing the JSON v2 data.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    /// <exception cref="JsonException"></exception>
    public static CsfData LoadFromJsonV2(Stream input)
        => JsonSerializer.Deserialize(input, CsfJsonV2JsonSerializerContext.Default.CsfData)
            ?? throw new JsonException("Failed to deserialize JSON v2 data.");

    /// <summary>
    /// Loads <see cref="CsfData"/> from a JSON v1 stream.
    /// </summary>
    /// <param name="input">The stream containing the JSON v1 data.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    /// <exception cref="JsonException"></exception>
    public static CsfData LoadFromJsonV1(Stream input)
        => JsonSerializer.Deserialize(input, CsfJsonV1JsonSerializerContext.Default.CsfData)
            ?? throw new JsonException("Failed to deserialize JSON v1 data.");

    /// <summary>
    /// Loads <see cref="CsfData"/> from an XML stream.
    /// </summary>
    /// <param name="input">The stream containing the XML data.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    public static CsfData LoadFromXml(Stream input)
        => CsfXmlV1Reader.Read(input);

    /// <summary>
    /// Loads <see cref="CsfData"/> from a stream in the specified format.
    /// </summary>
    /// <param name="input">The stream containing the serialized CSF data.</param>
    /// <param name="format">The format of the data in the stream.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="format"/> is not recognized.</exception>
    public static CsfData LoadFrom(Stream input, CsfFormat format)
    {
        Func<Stream, CsfData> func = format switch
        {
            CsfFormat.Csf => LoadFromCsf,
            CsfFormat.Yaml => LoadFromYaml,
            CsfFormat.JsonV2 => LoadFromJsonV2,
            CsfFormat.JsonV1 => LoadFromJsonV1,
            CsfFormat.Xml => LoadFromXml,
            _ => throw new NotSupportedException(),
        };
        return func(input);
    }

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in native CSF binary format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    public static void SaveToCsf(CsfData csf, Stream stream)
        => CsfWriter.WriteAllData(stream, csf);

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in YAML format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    public static void SaveToYaml(CsfData csf, Stream stream)
        => CsfYamlV1Writer.Write(stream, csf);

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in JSON v2 format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    public static void SaveToJsonV2(CsfData csf, Stream stream)
        => JsonSerializer.Serialize(stream, csf, CsfJsonV2JsonSerializerContext.Default.CsfData);

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in JSON v1 format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    public static void SaveToJsonV1(CsfData csf, Stream stream)
        => JsonSerializer.Serialize(stream, csf, CsfJsonV1JsonSerializerContext.Default.CsfData);

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in XML format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    public static void SaveToXml(CsfData csf, Stream stream)
        => CsfXmlV1Writer.Write(stream, csf);

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in the specified format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    /// <param name="format">The target output format.</param>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="format"/> is not recognized.</exception>
    public static void SaveTo(CsfData csf, Stream stream, CsfFormat format)
    {
        Action<CsfData, Stream> func = format switch
        {
            CsfFormat.Csf => SaveToCsf,
            CsfFormat.Yaml => SaveToYaml,
            CsfFormat.JsonV2 => SaveToJsonV2,
            CsfFormat.JsonV1 => SaveToJsonV1,
            CsfFormat.Xml => SaveToXml,
            _ => throw new NotSupportedException(),
        };

        func(csf, stream);
    }

    /// <summary>
    /// Guesses the <see cref="CsfFormat"/> of a file based on its extension.
    /// </summary>
    /// <param name="input">The file to examine.</param>
    /// <returns>The detected <see cref="CsfFormat"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the file extension does not match any known format.</exception>
    public static CsfFormat GuessInputFormat(FileInfo input)
    {
        if (input.Name.EndsWith(".csf", StringComparison.OrdinalIgnoreCase))
        {
            return CsfFormat.Csf;
        }
        else if (input.Name.EndsWith(".csf.yaml", StringComparison.OrdinalIgnoreCase)
            || input.Name.EndsWith(".csf.yml", StringComparison.OrdinalIgnoreCase))
        {
            return CsfFormat.Yaml;
        }
        else if (input.Name.EndsWith(".v2.csf.json", StringComparison.OrdinalIgnoreCase)
            || input.Name.EndsWith(".csf.v2.json", StringComparison.OrdinalIgnoreCase))
        {
            return CsfFormat.JsonV2;
        }
        else if (input.Name.EndsWith(".v1.csf.json", StringComparison.OrdinalIgnoreCase)
            || input.Name.EndsWith(".csf.v1.json", StringComparison.OrdinalIgnoreCase))
        {
            return CsfFormat.JsonV1;
        }
        else if (input.Name.EndsWith(".csf.xaml", StringComparison.OrdinalIgnoreCase)
            || input.Name.EndsWith(".csf.xml", StringComparison.OrdinalIgnoreCase))
        {
            return CsfFormat.Xml;
        }
        else
        {
            throw new NotSupportedException("无法分析出当前文件的格式");
        }
    }

    /// <summary>
    /// Returns the default output format for a given input format.
    /// <br />
    /// Typically the inverse: CSF → YAML, anything else → CSF.
    /// </summary>
    /// <param name="inputFormat">The input format.</param>
    /// <returns>The suggested output format.</returns>
    public static CsfFormat GuessOutputFormat(CsfFormat inputFormat) => inputFormat is CsfFormat.Csf ? CsfFormat.Yaml : CsfFormat.Csf;
}
