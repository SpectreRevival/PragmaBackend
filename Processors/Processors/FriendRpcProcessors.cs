using Packets;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class GetFriendListAndRegisterOnlineProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetFriendListAndRegisterOnlineProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("FriendRpc.GetFriendListAndRegisterOnlineV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        Model.FriendsList? existingFriends = await Model.FriendsList.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        Model.FriendsList friends = existingFriends ?? Model.FriendsList.CreateDefault(ConnectionHandler.PlayerId);

        if (existingFriends == null)
        {
            await friends.SyncToDatabase();
        }

        GetFriendsListAndRegisterOnlineResponse response = new() { FriendList = friends.ToPacket() };

        Model.PlayerPresence presence = await Model.PlayerPresence.RetrieveFromDatabase(ConnectionHandler.PlayerId)
                                        ?? Model.PlayerPresence.CreateDefault(ConnectionHandler.PlayerId);
        presence.BasicStatus = Model.PlayerBasicPresence.Online;
        presence.LastUpdatedTime = DateTimeOffset.UtcNow;
        await presence.SyncToDatabase();
        await FriendPresenceNotifications.SendPresenceUpdateToFriends(ConnectionHandler.PlayerId, presence);

        return SpectreWebsocketMessage.From(response);
    }
}

public class FriendSetPresenceProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public FriendSetPresenceProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("FriendRpc.SetPresenceV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        Model.PlayerPresence presence = await Model.PlayerPresence.RetrieveFromDatabase(ConnectionHandler.PlayerId)
                                        ?? Model.PlayerPresence.CreateDefault(ConnectionHandler.PlayerId);

        presence.BasicStatus = ParseBasicPresence(Packet.RequestPayload);
        presence.LastUpdatedTime = DateTimeOffset.UtcNow;
        await presence.SyncToDatabase();
        await FriendPresenceNotifications.SendPresenceUpdateToFriends(ConnectionHandler.PlayerId, presence);

        return SpectreWebsocketMessage.From(new JsonObject { ["response"] = "Ok" });
    }

    private static Model.PlayerBasicPresence ParseBasicPresence(JsonObject payload)
    {
        JsonNode? rawBasicPresence = payload["basicPresence"];
        if (rawBasicPresence == null)
        {
            return Model.PlayerBasicPresence.Online;
        }

        if (rawBasicPresence.GetValueKind() == System.Text.Json.JsonValueKind.Number)
        {
            int basicPresenceValue = rawBasicPresence.GetValue<int>();
            return Enum.IsDefined(typeof(Model.PlayerBasicPresence), basicPresenceValue)
                ? (Model.PlayerBasicPresence)basicPresenceValue
                : Model.PlayerBasicPresence.Online;
        }

        if (rawBasicPresence.GetValueKind() == System.Text.Json.JsonValueKind.String
            && Enum.TryParse(rawBasicPresence.GetValue<string>(), ignoreCase: true,
                out Model.PlayerBasicPresence basicPresence))
        {
            return basicPresence;
        }

        return Model.PlayerBasicPresence.Online;
    }
}