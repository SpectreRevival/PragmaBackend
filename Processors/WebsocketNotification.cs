using Google.Protobuf;
using System.Text.Json.Nodes;

namespace Processors;

public class WebsocketNotification
{
    private readonly string data;
    private readonly SpectreRpcType notificationType;

    public WebsocketNotification(string data, SpectreRpcType notificationType)
    {
        this.data = data;
        this.notificationType = notificationType;
    }

    public WebsocketNotification(IMessage data, SpectreRpcType notificationType)
    {
        JsonFormatter outFormatter = new(
    new JsonFormatter.Settings(true)
    .WithFormatDefaultValues(true)
    .WithFormatEnumsAsIntegers(true)
    .WithIndentation("")
    .WithPreserveProtoFieldNames(true)
);
        this.data = outFormatter.Format(data);
        this.notificationType = notificationType;
    }

    public WebsocketNotification(JsonObject data, SpectreRpcType notificationType)
    {
        this.data = data.ToJsonString();
        this.notificationType = notificationType;
    }

    public string GetData()
    {
        return data;
    }

    public SpectreRpcType GetRpcType()
    {
        return notificationType;
    }
}