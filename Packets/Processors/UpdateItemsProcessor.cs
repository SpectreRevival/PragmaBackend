using Model;
using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class UpdateItemsProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public UpdateItemsProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("InventoryRpc.UpdateItemsV0Request");
    }

    private static InstancedItem ConvertCustomizedInstanced(CustomizedInstancedItem item)
    {
        InstancedItem ret = new();
        ret.InstanceId = item.InstanceId.ToString();
        ret.CatalogId = item.CatalogId;
        ret.Ext.Viewed = item.Viewed;
        InstancedCustomizationData altData = new();
        foreach(var altChannel in item.AlterationChannels)
        {
            var newChannel = new AlterationChannel();
            newChannel.ChannelId = altChannel.ChannelId;
            newChannel.OwnedAlterations.AddRange(altChannel.Alterations);
            altData.InstancedAlterationChannels.Add(newChannel);
        }
        ret.Ext.InstancedCustomizationData = altData;
        return ret;
    }

    private static async Task<BattlepassData> ConvertAndCreateBpTrackerItem(Guid playerId)
    {
        Model.BattlepassData bpData = await Model.BattlepassData.RetrieveFromDatabase(playerId);
        BattlepassData packet = new();
        foreach(var activePass in bpData.ActiveBattlePasses)
        {
            packet.ActiveBattlePasses.Add(activePass.ToString());
        }
        foreach(var quest in bpData.BattlepassQuests)
        {
            packet.BpQuests.Add(quest.ToString());
        }
        foreach(var activeQuest in bpData.ActiveBattlepassQuests)
        {
            packet.ActiveBpQuests.Add(activeQuest.ToString());
        }
        packet.DebugSeasonOffsetMillis = "0";
        Model.SeasonEntry entry = await Model.SeasonEntry.RetrieveFromDatabase(1);
        SeasonEntry itemEntry = new();
        itemEntry.SeasonNumber = entry.SeasonNumber;
        itemEntry.StartTimestampMillis = entry.StartTime.ToUnixTimeMilliseconds().ToString();
        itemEntry.EndTimestampMillis = entry.EndTime.ToUnixTimeMilliseconds().ToString();
        itemEntry.LastWeekEndTimestampMillis = entry.LastWeekEnd.ToUnixTimeMilliseconds().ToString();
        itemEntry.FirstWeekStartTimestampMillis = entry.FirstWeekStart.ToUnixTimeMilliseconds().ToString();
        itemEntry.NumberOfWeeksInSeason = entry.NumberOfWeeksInSeason;
        packet.SeasonEntry = itemEntry;
        return packet;
    }

    private static async Task<InstancedItem> ConvertIndividualProg(IndividualTrackedProgression prog)
    {
        InstancedItem ret = new();
        ret.InstanceId = prog.PlayerId.ToString();
        ret.CatalogId = "MtnManualItem:ProgressionTracker";
        TrackedProgression itemProg = new();
        itemProg.ActiveEndorsement = prog.ActiveEndorsement.ToString();
        foreach(var daily in prog.ActiveDailyQuests)
        {
            itemProg.ActiveDailyQuests.Add(daily.ToString());
        }
        foreach(var weekly in prog.ActiveWeeklyQuests)
        {
            itemProg.ActiveWeeklyQuests.Add(weekly.ToString());
        }
        foreach(var ev in prog.ActiveEventQuests)
        {
            itemProg.ActiveEventQuests.Add(ev.ToString());
        }
        itemProg.LastRolloverTimestamp = prog.LastRolloverTimestamp.ToUnixTimeMilliseconds().ToString();
        itemProg.BpTrackerData = await ConvertAndCreateBpTrackerItem(prog.PlayerId);
        ret.Ext.TrackedProgression = itemProg;
        return ret;
    }
   
    private static StackableItem ConvertStackableItem(Model.StackableItem item)
    {
        StackableItem ret = new();
        ret.Amount = item.Amount.ToString();
        ret.InstanceId = item.InstanceId.ToString();
        ret.CatalogId = item.CatalogId;
        return ret;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        UpdateItemsRequest req = Packet.GetPayloadAsMessage<UpdateItemsRequest>();
        UpdateItemsResponse res = new();
        InventorySegment segment = new();
        InventoryDelta delta = new();
        res.Segment = segment;
        res.Delta = delta;
        foreach(var instancedItemUpdate in req.InstancedItemUpdates)
        {
            if (instancedItemUpdate.Ext.ProgressionTrackerUpdate != null)
            {
                IndividualTrackedProgression prog = await IndividualTrackedProgression.RetrieveFromDatabase(ConnectionHandler.PlayerId);
                InstancedItem init = await ConvertIndividualProg(prog);
                InstancedDelta itemDelta = new();
                itemDelta.Initial = init;
                prog.ActiveEndorsement = Guid.Parse(instancedItemUpdate.Ext.ProgressionTrackerUpdate.NewActiveEndorsement);
                await prog.SyncToDatabase();
                InstancedItem final = await ConvertIndividualProg(prog);
                segment.Instanced.Add(final);
                itemDelta.CatalogId = "MtnManualItem:ProgressionTracker";
                itemDelta.Operation = "UPDATED";
                itemDelta.Final = final;
                delta.Instanced.Add(itemDelta);
            }
            else if (instancedItemUpdate.Ext.SetViewed)
            {
                CustomizedInstancedItem item = await CustomizedInstancedItem.RetrieveFromDatabase(Guid.Parse(instancedItemUpdate.InstanceId));
                InstancedDelta itemDelta = new();
                itemDelta.Initial = ConvertCustomizedInstanced(item);
                item.Viewed = true;
                await item.SyncToDatabase();
                InstancedItem final = ConvertCustomizedInstanced(item);
                segment.Instanced.Add(final);
                itemDelta.CatalogId = item.CatalogId;
                itemDelta.Operation = "UPDATED";
                itemDelta.Final = final;
                delta.Instanced.Add(itemDelta);
            }
        }
        foreach(var stackableItemUpdate in req.StackableItemsUpdates)
        {
            Model.StackableItem item = await Model.StackableItem.RetrieveFromDatabase(Guid.Parse(stackableItemUpdate.InstanceId));
            StackableItem init = ConvertStackableItem(item);
            StackableDelta itemDelta = new();
            itemDelta.Initial = init;
            item.Amount = long.Parse(stackableItemUpdate.NewAmount);
            await item.SyncToDatabase();
            StackableItem final = ConvertStackableItem(item);
            segment.Stackables.Add(final);
            itemDelta.CatalogId = item.CatalogId;
            itemDelta.Final = final;
            itemDelta.Operation = "UPDATED";
            delta.Stackables.Add(itemDelta);
        }
        return SpectreWebsocketMessage.From(res);
    }
}