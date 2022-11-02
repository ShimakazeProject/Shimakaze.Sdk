using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Sdk.Preprocessor.Ini;

namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;
internal sealed class PragmaCommand : IPreprocessorCommand
{
    private const string Types_Dictionary_String_Dictionary_String_String = "Types";
    private const string DEFINE_GENERATE_SECTION_HEAD = "GENERATE_SECTION_HEAD";

    public string Command => "pragma";
    private bool _generateHead = false;

    public Task InitializeAsync(IniPreprocessor preprocessor)
    {
        _generateHead = preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines).Contains(DEFINE_GENERATE_SECTION_HEAD);
        return Task.CompletedTask;
    }

    public async Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        if (args.Length < 1)
            throw new ArgumentException("pragma must have 1 parameter");

        switch (args[0])
        {
            case "type":
                {
                    var currentFile = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile);
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(currentFile.Peek());

                    if (_generateHead)
                    {
                        var output = preprocessor.GetVariable<TextWriter>(PreprocessorVariableNames.OutputStream);
                        await output.WriteLineAsync($"[{fileNameWithoutExt}]");
                    }

                    switch (args.Length)
                    {
                        case 2:
                            AddType(preprocessor, args[1], fileNameWithoutExt);
                            break;
                        case 3:
                            AddType(preprocessor, args[1], args[2], fileNameWithoutExt);
                            break;
                    }
                }
                break;
            default:
                break;
        }
    }

    public bool IsPostProcessing => true;

    public async Task PostProcessingAsync(IniPreprocessor preprocessor)
    {
        var map = GetTypeMap(preprocessor);
        var output = preprocessor.GetVariable<TextWriter>(PreprocessorVariableNames.OutputStream);

        await map.EachAsync(async i =>
        {
            await output.WriteLineAsync();
            await output.WriteLineAsync($"[{i.Key}]");
            await i.Value.EachAsync(kvp => output.WriteLineAsync($"{kvp.Key}={kvp.Value}"));
        });
    }

    private static Dictionary<string, Dictionary<string, string>> GetTypeMap(IniPreprocessor preprocessor)
    {
        if (!preprocessor.Variables.ContainsKey(Types_Dictionary_String_Dictionary_String_String))
        {
            preprocessor.Variables[Types_Dictionary_String_Dictionary_String_String] = new Dictionary<string, Dictionary<string, string>>();
        }
        return preprocessor.GetVariable<Dictionary<string, Dictionary<string, string>>>(Types_Dictionary_String_Dictionary_String_String);
    }
    private static void AddType(IniPreprocessor preprocessor, string type, string key, string value)
    {
        var map = GetTypeMap(preprocessor);
        if (!map.TryGetValue(type, out var section))
        {
            section = map[type] = new();
        }
        section.Add(key, value);
    }
    private static void AddType(IniPreprocessor preprocessor, string type, string value)
    {
        var map = GetTypeMap(preprocessor);
        if (!map.TryGetValue(type, out var section))
        {
            section = map[type] = new();
        }
        section.Add(section.Count.ToString(), value);
    }
}
