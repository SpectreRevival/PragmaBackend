using Google.Protobuf;
using System.Text.Json.Nodes;

namespace Processors;

public class SpectreWebsocketMessage
{
    private readonly string _data;
    private readonly WebsocketNotification[] _postSendNotifications;
    private readonly Func<Task>[] _postResponseActions;

    private SpectreWebsocketMessage(string data, WebsocketNotification[] postSendNotifications, Func<Task>[] postResponseActions)
    {
        _data = data;
        _postSendNotifications = postSendNotifications;
        _postResponseActions = postResponseActions;
    }

    public static SpectreWebsocketMessage From(string data, WebsocketNotification[]? postSendNotifications = null, Func<Task>[]? postResponseActions = null)
    {
        return new SpectreWebsocketMessage(data, postSendNotifications ?? [], postResponseActions ?? []);
    }

    public static SpectreWebsocketMessage From(IMessage protoMessage, WebsocketNotification[]? postSendNotifications = null, Func<Task>[]? postResponseActions = null)
    {
        JsonFormatter outFormatter = new(
    new JsonFormatter.Settings(true)
    .WithFormatDefaultValues(true)
    .WithFormatEnumsAsIntegers(true)
    .WithIndentation("")
    .WithPreserveProtoFieldNames(true)
);
        return new SpectreWebsocketMessage(outFormatter.Format(protoMessage), postSendNotifications ?? [], postResponseActions ?? []);
    }

    public static SpectreWebsocketMessage From(JsonObject json, WebsocketNotification[]? postSendNotifications = null, Func<Task>[]? postResponseActions = null)
    {
        return new SpectreWebsocketMessage(json.ToJsonString(), postSendNotifications ?? [], postResponseActions ?? []);
    }

    public static SpectreWebsocketMessage Empty()
    {
        return new SpectreWebsocketMessage("{}", [], []);
    }

    public string GetData()
    {
        return _data;
    }

    public WebsocketNotification[] GetNotifications()
    {
        return _postSendNotifications;
    }

    public async Task RunPostResponseActionsAsync()
    {
        foreach (Func<Task> action in _postResponseActions)
        {
            await action();
        }
    }
}