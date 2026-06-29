using Model;
using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors;

public class GetInventoryV2Processor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetInventoryV2Processor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("InventoryRpc.GetInventoryV2Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        Inventory resFinal = new();
        FullInventory res = new();
        NpgsqlCommand stackablesCmd = PostgresDatabase.LoadCommandFromFile("get_all_stackables_for_player.sql");
        stackablesCmd.Parameters.AddWithValue("player_id", ConnectionHandler.PlayerId);
        using var stackablesReader = await stackablesCmd.ExecuteReaderAsync();
        while (await stackablesReader.ReadAsync())
        {
            Guid stackableInstanceId = stackablesReader.GetGuid(0);
            Model.StackableItem item = await Model.StackableItem.RetrieveFromDatabase(stackableInstanceId);
            res.Stackables.Add(item.ToPacket());
        }
        NpgsqlCommand customizedInstancedCmd = PostgresDatabase.LoadCommandFromFile("get_all_customized_instanced_for_player.sql");
        using var customizedInstancedReader = await customizedInstancedCmd.ExecuteReaderAsync();
        while (await customizedInstancedReader.ReadAsync())
        {
            Guid instanceId = customizedInstancedReader.GetGuid(0);
            var item = await CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
            res.Instanced.Add(item.ToPacket());
        }
        NpgsqlCommand progTrackingCmd = PostgresDatabase.LoadCommandFromFile("get_all_progression_tracking_items.sql");
        using var progTrackingReader = await progTrackingCmd.ExecuteReaderAsync();
        while (await progTrackingReader.ReadAsync())
        {
            Guid instanceId = progTrackingReader.GetGuid(0);
            var item = await ProgressionTrackingItem.RetrieveFromDatabase(instanceId);
            res.Instanced.Add(item.ToPacket());
        }
        NpgsqlCommand sponsorTrackingCmd = PostgresDatabase.LoadCommandFromFile("get_all_sponsor_tracking_items.sql");
        using var sponsorTrackingReader = await sponsorTrackingCmd.ExecuteReaderAsync();
        while (await sponsorTrackingReader.ReadAsync())
        {
            Guid instanceId = sponsorTrackingReader.GetGuid(0);
            var item = await SponsorUnlockTrackerItem.RetrieveFromDatabase(instanceId);
            res.Instanced.Add(item.ToPacket());
        }
        Model.TeamTrackedProgression teamProg = await Model.TeamTrackedProgression.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        res.Instanced.Add(teamProg.ToProgressionTrackerItem());
        Model.IndividualTrackedProgression individualProg = await Model.IndividualTrackedProgression.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        res.Instanced.Add(await individualProg.ToProgressionTrackerItem());
        res.Version = (await Model.ProfileData.RetrieveFromDatabase(ConnectionHandler.PlayerId)).InventoryVersion.ToString();
        resFinal.Full = res;
        return SpectreWebsocketMessage.From(resFinal);
    }
}