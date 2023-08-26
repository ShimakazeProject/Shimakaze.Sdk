using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Sharprompt;

using Shimakaze.Sdk.Common;
using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Converter;
using Shimakaze.Sdk.IO.Csf;
using Shimakaze.Sdk.IO.Csf.Json;
using Shimakaze.Sdk.IO.Csf.Xml;
using Shimakaze.Sdk.IO.Csf.Yaml;

if (args is { Length: < 1 })
    throw new ArgumentException("参数太少");

string input = args[0];

await using var ifs = File.OpenRead(input);

ServiceCollection services = new();

var formats = new[]{
    "Yaml",
    "JsonV2",
    "JsonV1",
    "Xml",
    "Csf"
};
string current;
string defaultValue;
if (input.EndsWith(".csf", StringComparison.OrdinalIgnoreCase))
{
    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfReader(ifs));
    current = "Csf";
    defaultValue = "Yaml";
}
else if (input.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
{
    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfYamlV1Reader(ifs));
    current = "Yaml";
    defaultValue = "Csf";
}
else if (input.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
{
    var protocol = await JsonSerializer.DeserializeAsync<ProtocolObject>(ifs);
    ifs.Seek(0, SeekOrigin.Begin);
    JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };
    switch (protocol?.Protocol)
    {
        case 2:
            services.AddSingleton<AsyncReader<CsfDocument>>(new CsfJsonV2Reader(ifs, options));
            current = "JsonV2";
            break;
        case 1:
            services.AddSingleton<AsyncReader<CsfDocument>>(new CsfJsonV1Reader(ifs, options));
            current = "JsonV1";
            break;
        default:
            Console.WriteLine(protocol?.Protocol);
            throw new NotSupportedException();
    }
    defaultValue = "Csf";
}
else if (input.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
{
    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfXmlV1Reader(ifs));
    current = "Xml";
    defaultValue = "Csf";
}
else
{
    throw new NotSupportedException();
}

var selected = Prompt.Select("请选择要转换的格式", formats.Where(i => i != current), defaultValue: defaultValue);
var output = args.Length > 1
    ? args[1]
    : Prompt.Input<string>("请输入生成的文件的路径", selected switch
    {
        "Yaml" => $"{input}.yaml",
        "JsonV2" => $"{input}.json",
        "JsonV1" => $"{input}.json",
        "Xml" => $"{input}.xml",
        "Csf" => $"{input}.csf",
        _ => throw new NotSupportedException()
    });
await using var ofs = File.Create(output);
services.AddSingleton<AsyncWriter<CsfDocument>>(selected switch
{
    "Yaml" => new CsfYamlV1Writer(ofs),
    "JsonV2" => new CsfJsonV2Writer(ofs, new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
    "JsonV1" => new CsfJsonV1Writer(ofs, new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
    "Xml" => new CsfXmlV1Writer(ofs, new() { Indent = true }),
    "Csf" => new CsfWriter(ofs),
    _ => throw new NotSupportedException()
});
await using var provider = services.BuildServiceProvider();
var csf = await provider.GetRequiredService<AsyncReader<CsfDocument>>().ReadAsync();
await provider.GetRequiredService<AsyncWriter<CsfDocument>>().WriteAsync(csf);