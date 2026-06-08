using Google.Protobuf;
using System.Text.Json.Nodes;

namespace Packets;

public class SpectreWebsocketMessage
{
    private readonly string _data;

    private SpectreWebsocketMessage(string data)
    {
        _data = data;
    }

    public static SpectreWebsocketMessage From(string data)
    {
        return new SpectreWebsocketMessage(data);
    }

    public static SpectreWebsocketMessage From(IMessage protoMessage)
    {
        JsonFormatter outFormatter = new(
    new JsonFormatter.Settings(true)
    .WithFormatDefaultValues(true)
    .WithFormatEnumsAsIntegers(true)
    .WithIndentation("")
    .WithPreserveProtoFieldNames(true)
);
        return new SpectreWebsocketMessage(outFormatter.Format(protoMessage));
    }

    public static SpectreWebsocketMessage From(JsonObject json)
    {
        return new SpectreWebsocketMessage(json.ToJsonString());
    }

    public string GetData()
    {
        return _data;
    }
}