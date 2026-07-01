using Google.Protobuf;
using System.Text.Json.Nodes;

namespace Processors;

public class SpectreWebsocketMessage
{
    private readonly string _data;
    private readonly WebsocketNotification[] _postSendNotifications;

    private SpectreWebsocketMessage(string data, WebsocketNotification[] postSendNotifications)
    {
        _data = data;
        _postSendNotifications = postSendNotifications;
    }

    public static SpectreWebsocketMessage From(string data, WebsocketNotification[]? postSendNotifications = null)
    {
        return new SpectreWebsocketMessage(data, postSendNotifications ?? []);
    }

    public static SpectreWebsocketMessage From(IMessage protoMessage, WebsocketNotification[]? postSendNotifications = null)
    {
        JsonFormatter outFormatter = new(
    new JsonFormatter.Settings(true)
    .WithFormatDefaultValues(true)
    .WithFormatEnumsAsIntegers(true)
    .WithIndentation("")
    .WithPreserveProtoFieldNames(true)
);
        return new SpectreWebsocketMessage(outFormatter.Format(protoMessage), postSendNotifications ?? []);
    }

    public static SpectreWebsocketMessage From(JsonObject json, WebsocketNotification[]? postSendNotifications = null)
    {
        return new SpectreWebsocketMessage(json.ToJsonString(), postSendNotifications ?? []);
    }

    public static SpectreWebsocketMessage Empty()
    {
        return new SpectreWebsocketMessage("{}", []);
    }

    public string GetData()
    {
        return _data;
    }

    public WebsocketNotification[] GetNotifications()
    {
        return _postSendNotifications;
    }
}