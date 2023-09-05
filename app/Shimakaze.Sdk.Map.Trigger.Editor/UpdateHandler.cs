using Shimakaze.Sdk.JsonRPC.Server;

namespace Shimakaze.Sdk.Map.Trigger;


[Handler]
public sealed class UpdateHandler
{
    private readonly Context _ctx;

    public UpdateHandler(Context ctx)
    {
        _ctx = ctx;
    }

    [Method]
    public UpdateType Tag(Guid sessionId, string tagId, NullableTag data)
    {
        var ctx = _ctx.Sessions[sessionId];
        if (ctx.Tags.TryGetValue(tagId, out var tag))
        {
            ctx.Tags[tagId] = data.Update(tag);
            return UpdateType.Update;
        }
        else
        {
            ctx.Tags[tagId] = data;
            return UpdateType.Add;
        }
    }

    [Method]
    public UpdateType Trigger(Guid sessionId, string triggerId, NullableTrigger data)
    {
        var ctx = _ctx.Sessions[sessionId];
        if (ctx.Triggers.TryGetValue(triggerId, out var trigger))
        {
            ctx.Triggers[triggerId] = data.Update(trigger);
            return UpdateType.Update;
        }
        else
        {
            ctx.Triggers[triggerId] = data;
            return UpdateType.Add;
        }
    }

    [Method]
    public UpdateType Event(Guid sessionId, string eventId, NullableTriggerEvent data)
    {
        var ctx = _ctx.Sessions[sessionId];
        if (ctx.Events.TryGetValue(eventId, out var @event))
        {
            ctx.Events[eventId] = data.Update(@event);
            return UpdateType.Update;
        }
        else
        {
            ctx.Events[eventId] = data;
            return UpdateType.Add;
        }
    }

    [Method]
    public UpdateType Action(Guid sessionId, string actionId, NullableTriggerAction data)
    {
        var ctx = _ctx.Sessions[sessionId];
        if (ctx.Actions.TryGetValue(actionId, out var action))
        {
            ctx.Actions[actionId] = data.Update(action);
            return UpdateType.Update;
        }
        else
        {
            ctx.Actions[actionId] = data;
            return UpdateType.Add;
        }
    }
}
