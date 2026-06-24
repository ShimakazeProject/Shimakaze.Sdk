using System.Text;

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
    /// Loads <see cref="CsfData"/> from a stream in the specified format.
    /// </summary>
    /// <param name="input">The stream containing the serialized CSF data.</param>
    /// <param name="format">The format of the data in the stream.</param>
    /// <returns>The deserialized <see cref="CsfData"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="format"/> is not recognized.</exception>
    public static async Task<CsfData> LoadFromAsync(Stream input, CsfFormat format) => await (format switch
    {
        CsfFormat.Csf => Task.Run(() => CsfReader.ReadAllData(input)),
        CsfFormat.Yaml => Task.Run(() =>
        {
            using StreamReader sr = new(input, Encoding.UTF8, true, 128, true);
            return CsfYamlV1Reader.Read(sr);
        }),
        CsfFormat.JsonV2 => CsfJsonV2.ReadAllDataAsync(input),
        CsfFormat.JsonV1 => CsfJsonV1.ReadAllDataAsync(input),
        CsfFormat.Xml => Task.Run(() =>
        {
            using StreamReader sr = new(input, Encoding.UTF8, true, 128, true);
            return CsfXmlV1Reader.Read(sr);
        }),
        _ => throw new NotSupportedException(),
    });

    /// <summary>
    /// Saves <see cref="CsfData"/> to a stream in the specified format.
    /// </summary>
    /// <param name="csf">The CSF data to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    /// <param name="format">The target output format.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="format"/> is not recognized.</exception>
    public static async Task SaveToAsync(CsfData csf, Stream stream, CsfFormat format)
    {
        Func<CsfData, Task> writer = format switch
        {
            CsfFormat.Yaml => async csf => await Task.Run(async () =>
            {
                await Task.Yield();
                using StreamWriter sw = new(stream, Encoding.UTF8, 128, true);
                CsfYamlV1Writer.Write(sw, csf);
            }),
            CsfFormat.JsonV2 => async csf => await CsfJsonV2.WriteAllDataAsync(stream, csf),
            CsfFormat.JsonV1 => async csf => await CsfJsonV1.WriteAllDataAsync(stream, csf),
            CsfFormat.Xml => async csf =>
            {
                await Task.Yield();
                using StreamWriter sw = new(stream, Encoding.UTF8, 128, true);
                CsfXmlV1Writer.Write(sw, csf);
            }
            ,
            CsfFormat.Csf => async csf => await Task.Run(() => CsfWriter.WriteAllData(stream, csf)),
            _ => throw new NotSupportedException()
        };

        await writer(csf);
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
