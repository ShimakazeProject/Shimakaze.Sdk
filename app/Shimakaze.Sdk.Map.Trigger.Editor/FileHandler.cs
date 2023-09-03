using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.IO.Ini;
using Shimakaze.Sdk.JsonRPC.Server;

namespace Shimakaze.Sdk.Map.Trigger;

[Handler]
public sealed class FileHandler
{
    private readonly Dictionary<Guid, Context> _context = new();

    [Method]
    public async Task<Guid> OpenFileAsync(string path)
    {
        Guid key = Guid.NewGuid();
        while (_context.ContainsKey(key))
            key = Guid.NewGuid();

        await using Stream iniFile = File.OpenRead(path);
        using IniReader reader = new(iniFile);
        IniDocument ini = await reader.ReadAsync().ConfigureAwait(false);
        _context[key] = new()
        {
            Path = path,
            Ini = ini,
        };
        return key;
    }

    [Method]
    public TagsAndTriggers GetTagsAndTriggers(Guid key)
    {
        var ini = _context[key].Ini;
        IniSection tags = ini["Tags"];
        IniSection triggers = ini["Triggers"];
        IniSection events = ini["Events"];
        IniSection actions = ini["Actions"];

        return new(
            tags.Keys.ToDictionary(
                i => i,
                i => TagData.Parse(tags[i])
            ),
            triggers.Keys.ToDictionary(
                i => i,
                i => new TriggerObjectData(
                    TriggerData.Parse(triggers[i]),
                    EventData.Parse(events[i]),
                    ActionData.Parse(actions[i])
                )
            )
        );
    }
}
