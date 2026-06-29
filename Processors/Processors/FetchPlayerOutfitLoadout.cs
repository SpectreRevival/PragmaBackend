using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class FetchPlayerOutfitLoadout : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public FetchPlayerOutfitLoadout(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnLoadoutServiceRpc.FetchPlayerOutfitLoadoutsV1Request");
    }

    private static Packets.OutfitData OutfitDataFromModel(Model.OutfitData model)
    {
        Packets.OutfitData ret = new();
        foreach (Model.ActiveAlterationData alt in model.AlterationData)
        {
            Packets.ActiveAlterationData packetAlt = new()
            {
                AlterationId = alt.AlterationId,
                ChannelId = alt.ChannelId
            };
            ret.AlterationData.Add(packetAlt);
        }
        ret.ItemInstanceId = model.ItemInstanceId.ToString();
        ret.ItemCatalogId = model.ItemCatalogId;
        return ret;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchLoadoutsRequest req = Packet.GetPayloadAsMessage<FetchLoadoutsRequest>();
        NpgsqlCommand cmd = PostgresDatabase.Get().GetRaw().CreateCommand("SELECT loadout_id FROM outfit_loadouts WHERE player_id = @player_id");
        cmd.Parameters.AddWithValue("player_id", Guid.Parse(req.PlayerId));
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        OutfitLoadouts responseData = new();
        while (await reader.ReadAsync())
        {
            Guid loadoutId = reader.GetGuid(0);
            Model.OutfitLoadout curLoadout = await Model.OutfitLoadout.RetrieveFromDatabase(loadoutId);
            Packets.OutfitLoadout packetLoadout = new()
            {
                LoadoutId = loadoutId.ToString(),
                PlayerId = ConnectionHandler.PlayerId.ToString(),
                FaceAccessoryData = OutfitDataFromModel(curLoadout.FaceAccessory),
                FaceStyleData = OutfitDataFromModel(curLoadout.FaceStyle),
                HeadData = OutfitDataFromModel(curLoadout.Head),
                HairData = OutfitDataFromModel(curLoadout.Hair),
                OutfitData = OutfitDataFromModel(curLoadout.Outfit)
            };
            responseData.Loadouts.Add(packetLoadout);
        }
        responseData.Message.Add($"Loadouts: {responseData.Loadouts.Count}");
        return SpectreWebsocketMessage.From(responseData);
    }
}