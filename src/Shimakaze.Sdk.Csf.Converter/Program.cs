using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.Extensions.DependencyInjection;

using Sharprompt;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;

if (args is { Length: < 1 })
    throw new ArgumentException("参数太少");

string input = args[0];

using Stream ifs = File.OpenRead(input);

ServiceCollection services = new();

string current;
string defaultValue;

string[] supportFormats = [
    "Yaml",
    "JsonV2",
    "JsonV1",
    "Xml",
    "Csf"
];
Func<string, string> supportFormatNames = value => value switch
{
    "Yaml" => "Shimakaze.Sdk 定义的 Yaml 格式",
    "JsonV2" => "Shimakaze.Sdk 定义的 Json 格式 第2版",
    "JsonV1" => "Shimakaze.Sdk 定义的 Json 格式 第1版",
    "Xml" => "Shimakaze.Sdk 定义的 Xml 格式",
    "Csf" => "游戏引擎所使用的 Csf 二进制格式",
    _ => "未知",
};

switch (Path.GetExtension(input).ToLowerInvariant())
{
    case ".csf":
        services.AddSingleton<ICsfReader>(new CsfReader(ifs));
        current = "Csf";
        defaultValue = "Yaml";
        break;
    case ".yml":
    case ".yaml":
        services.AddSingleton<ICsfReader>(new CsfYamlV1Reader(new StreamReader(ifs)));
        current = "Yaml";
        defaultValue = "Csf";
        break;
    case ".json":
        {
            JsonObject? root = JsonNode.Parse(ifs, default, new()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            })?.Root.AsObject();

            if (root is null
                || !root.TryGetPropertyValue("protocol", out JsonNode? node)
                || node is null
                || !node.AsValue().TryGetValue(out int protocol))
                throw new NotSupportedException();

            ifs.Seek(0, SeekOrigin.Begin);
            JsonSerializerOptions options = new()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            switch (protocol)
            {
                case 2:
                    services.AddSingleton<ICsfReader>(new CsfJsonV2Reader(ifs, options));
                    current = "JsonV2";
                    break;
                case 1:
                    services.AddSingleton<ICsfReader>(new CsfJsonV1Reader(ifs, options));
                    current = "JsonV1";
                    break;
                default:
                    throw new NotSupportedException();
            }
            defaultValue = "Csf";
        }
        break;
    case ".xml":
    case ".xaml":
        services.AddSingleton<ICsfReader>(new CsfXmlV1Reader(new StreamReader(ifs)));
        current = "Xml";
        defaultValue = "Csf";
        break;
    default:
        {
            var tmp = Prompt.Select(
                "请选择当前文件的格式",
                supportFormats,
                textSelector: supportFormatNames);

            if (tmp is "Yaml") goto case ".yaml";
            else if (tmp is "JsonV2" or "JsonV1") goto case ".json";
            else if (tmp is "Xml") goto case ".xml";
            else if (tmp is "Csf") goto case ".csf";
            else throw new NotSupportedException();
        }
}


string selected = Prompt.Select(
    "请选择要转换的格式",
    supportFormats.Where(i => i != current),
    defaultValue: defaultValue,
    textSelector: supportFormatNames);

string output = args.Length > 1
    ? args[1]
    : Prompt.Input<string>(
        "请输入生成的文件的路径",
        selected switch
        {
            "Yaml" => $"{input}.yaml",
            "JsonV2" => $"{input}.json",
            "JsonV1" => $"{input}.json",
            "Xml" => $"{input}.xml",
            "Csf" => $"{input}.csf",
            _ => throw new NotSupportedException()
        });

using Stream ofs = File.Create(output);
services.AddSingleton<ICsfWriter>(selected switch
{
    "Yaml" => new CsfYamlV1Writer(new StreamWriter(ofs)),
    "JsonV2" => new CsfJsonV2Writer(ofs, new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
    "JsonV1" => new CsfJsonV1Writer(ofs, new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
    "Xml" => new CsfXmlV1Writer(new StreamWriter(ofs), new() { Indent = true }),
    "Csf" => new CsfWriter(ofs),
    _ => throw new NotSupportedException()
});
await using var provider = services.BuildServiceProvider();
var csf = await provider.GetRequiredService<ICsfReader>().ReadAsync();
await provider.GetRequiredService<ICsfWriter>().WriteAsync(csf);