using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

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

    private static OutfitData OutfitDataFromModel(Model.OutfitData model)
    {
        OutfitData ret = new();
        foreach (Model.ActiveAlterationData alt in model.AlterationData)
        {
            ActiveAlterationData packetAlt = new();
            packetAlt.AlterationId = alt.AlterationId;
            packetAlt.ChannelId = alt.ChannelId;
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
        await using var reader = await cmd.ExecuteReaderAsync();
        OutfitLoadouts responseData = new();
        while (await reader.ReadAsync())
        {
            Guid loadoutId = reader.GetGuid(0);
            Model.OutfitLoadout curLoadout = await Model.OutfitLoadout.RetrieveFromDatabase(loadoutId);
            OutfitLoadout packetLoadout = new();
            packetLoadout.LoadoutId = loadoutId.ToString();
            packetLoadout.PlayerId = ConnectionHandler.PlayerId.ToString();
            packetLoadout.FaceAccessoryData = OutfitDataFromModel(curLoadout.FaceAccessory);
            packetLoadout.FaceStyleData = OutfitDataFromModel(curLoadout.FaceStyle);
            packetLoadout.HeadData = OutfitDataFromModel(curLoadout.Head);
            packetLoadout.HairData = OutfitDataFromModel(curLoadout.Hair);
            packetLoadout.OutfitData = OutfitDataFromModel(curLoadout.Outfit);
            responseData.Loadouts.Add(packetLoadout);
        }
        responseData.Message.Add($"Loadouts: {responseData.Loadouts.Count}");
        return SpectreWebsocketMessage.From(responseData);
    }
}