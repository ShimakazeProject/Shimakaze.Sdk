using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.IO.Ini;
using Shimakaze.Sdk.JsonRPC.Server;

namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 文件操作
/// </summary>
[Handler]
public sealed class FileHandler
{
    private readonly Context _ctx;

    public FileHandler(Context ctx)
    {
        _ctx = ctx;
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [Method]
    public async Task<object> OpenAsync(string path)
    {
        Guid sessionId = Guid.NewGuid();
        while (_ctx.Sessions.ContainsKey(sessionId))
            sessionId = Guid.NewGuid();

        await using Stream iniFile = File.OpenRead(path);
        using IniReader reader = new(iniFile);
        IniDocument ini = await reader.ReadAsync().ConfigureAwait(false);
        IniSection tags = ini["Tags"];
        IniSection triggers = ini["Triggers"];
        IniSection events = ini["Events"];
        IniSection actions = ini["Actions"];

        _ctx.Sessions[sessionId] = new()
        {
            Path = path,
            Ini = ini,
            Tags = tags.Keys.ToDictionary(
                i => i,
                i => Tag.Parse(tags[i])
            ),
            Triggers = triggers.Keys.ToDictionary(
                i => i,
                i => Trigger.Parse(triggers[i])
            ),
            Events = events.Keys.ToDictionary(
                i => i,
                i => TriggerEvent.Parse(events[i])
            ),
            Actions = actions.Keys.ToDictionary(
                i => i,
                i => TriggerAction.Parse(actions[i])
            ),
        };

        return await GetAsync(sessionId);
    }


    [Method]
    public async Task<object> GetAsync(Guid sessionId)
    {
        var ctx = _ctx.Sessions[sessionId];

        return new
        {
            SessionId = sessionId,
            ctx.Tags,
            ctx.Triggers,
            ctx.Events,
            ctx.Actions,
        };

    }
}
