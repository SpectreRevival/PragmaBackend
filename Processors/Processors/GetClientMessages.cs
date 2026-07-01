using Model.Persistence;
using Npgsql;
using Packets;
using Processors;
using System.Diagnostics.CodeAnalysis;

namespace Processor.Processors;

public class GetClientMessages : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetClientMessages(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnInboxServiceRpc.GetMessagesClientV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("get_all_client_messages_for_player.sql");
        cmd.Parameters.AddWithValue("player_id", ConnectionHandler.PlayerId);
        using var reader = await cmd.ExecuteReaderAsync();
        GetMessagesResponse res = new GetMessagesResponse();
        while(await reader.ReadAsync())
        {
            Guid messageId = reader.GetGuid(0);
            Model.ClientMessage msg = await Model.ClientMessage.RetrieveFromDatabase(messageId);
            res.Messages.Add(msg.ToPacket());
        }
        return SpectreWebsocketMessage.From(res);
    }
}