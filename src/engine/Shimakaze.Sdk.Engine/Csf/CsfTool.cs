using System.Text;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;

namespace Shimakaze.Sdk.Engine.Csf;

internal static class CsfTool
{
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

    public static CsfFormat GuessOutputFormat(CsfFormat inputFormat) => inputFormat is CsfFormat.Csf ? CsfFormat.Yaml : CsfFormat.Csf;

}
