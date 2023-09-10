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

    [Method]
    public async Task SaveToAsync(Guid sessionId, string path)
    {
        var ctx = _ctx.Sessions[sessionId];
        var ini = ctx.Ini;
        IniSection tags = new() { Name = "Tags" };
        IniSection triggers = new() { Name = "Triggers" };
        IniSection events = new() { Name = "Events" };
        IniSection actions = new() { Name = "Actions" };

        foreach (var item in ctx.Tags)
            tags[item.Key] = item.Value.ToIniValue();
        foreach (var item in ctx.Triggers)
            triggers[item.Key] = item.Value.ToIniValue();
        foreach (var item in ctx.Events)
            events[item.Key] = item.Value.ToIniValue();
        foreach (var item in ctx.Actions)
            actions[item.Key] = item.Value.ToIniValue();

        ini["Tags"] = tags;
        ini["Triggers"] = triggers;
        ini["Events"] = events;
        ini["Actions"] = actions;

        await using Stream stream = File.Create(path);
        using IniWriter writer = new(stream);
        await writer.WriteAsync(ini);
    }

    [Method]
    public async Task SaveAsync(Guid sessionId)
    {
        var ctx = _ctx.Sessions[sessionId];
        await SaveToAsync(sessionId, ctx.Path);
    }
}
