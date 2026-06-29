using Model;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

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

    private static Packets.InstancedItem ConvertCustomizedInstanced(CustomizedInstancedItem item)
    {
        Packets.InstancedItem ret = new()
        {
            InstanceId = item.InstanceId.ToString(),
            CatalogId = item.CatalogId
        };
        ret.Ext.Viewed = item.Viewed;
        InstancedCustomizationData altData = new();
        foreach (Model.AlterationChannel altChannel in item.AlterationChannels)
        {
            Packets.AlterationChannel newChannel = new()
            {
                ChannelId = altChannel.ChannelId
            };
            newChannel.OwnedAlterations.AddRange(altChannel.Alterations);
            altData.InstancedAlterationChannels.Add(newChannel);
        }
        ret.Ext.InstancedCustomizationData = altData;
        return ret;
    }

    private static async Task<Packets.BattlepassData> ConvertAndCreateBpTrackerItem(Guid playerId)
    {
        Model.BattlepassData bpData = await Model.BattlepassData.RetrieveFromDatabase(playerId);
        Packets.BattlepassData packet = new();
        foreach (Guid activePass in bpData.ActiveBattlePasses)
        {
            packet.ActiveBattlePasses.Add(activePass.ToString());
        }
        foreach (Guid quest in bpData.BattlepassQuests)
        {
            packet.BpQuests.Add(quest.ToString());
        }
        foreach (Guid activeQuest in bpData.ActiveBattlepassQuests)
        {
            packet.ActiveBpQuests.Add(activeQuest.ToString());
        }
        packet.DebugSeasonOffsetMillis = "0";
        packet.SeasonEntry = Model.SeasonEntry.GetActive().ToPacket();
        return packet;
    }

    private static async Task<Packets.InstancedItem> ConvertIndividualProg(IndividualTrackedProgression prog)
    {
        Packets.InstancedItem ret = new()
        {
            InstanceId = prog.PlayerId.ToString(),
            CatalogId = "MtnManualItem:ProgressionTracker"
        };
        Packets.TrackedProgression itemProg = new()
        {
            ActiveEndorsement = prog.ActiveEndorsement.ToString()
        };
        foreach (Guid daily in prog.ActiveDailyQuests)
        {
            itemProg.ActiveDailyQuests.Add(daily.ToString());
        }
        foreach (Guid weekly in prog.ActiveWeeklyQuests)
        {
            itemProg.ActiveWeeklyQuests.Add(weekly.ToString());
        }
        foreach (Guid ev in prog.ActiveEventQuests)
        {
            itemProg.ActiveEventQuests.Add(ev.ToString());
        }
        itemProg.LastRolloverTimestamp = prog.LastRolloverTimestamp.ToUnixTimeMilliseconds().ToString();
        itemProg.BpTrackerData = await ConvertAndCreateBpTrackerItem(prog.PlayerId);
        ret.Ext.TrackedProgression = itemProg;
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
        foreach (InstancedItemUpdate? instancedItemUpdate in req.InstancedItemUpdates)
        {
            if (instancedItemUpdate.Ext.ProgressionTrackerUpdate != null)
            {
                IndividualTrackedProgression prog = await IndividualTrackedProgression.RetrieveFromDatabase(ConnectionHandler.PlayerId);
                Packets.InstancedItem init = await ConvertIndividualProg(prog);
                InstancedDelta itemDelta = new()
                {
                    Initial = init
                };
                prog.ActiveEndorsement = Guid.Parse(instancedItemUpdate.Ext.ProgressionTrackerUpdate.NewActiveEndorsement);
                await prog.SyncToDatabase();
                Packets.InstancedItem final = await ConvertIndividualProg(prog);
                segment.Instanced.Add(final);
                itemDelta.CatalogId = "MtnManualItem:ProgressionTracker";
                itemDelta.Operation = "UPDATED";
                itemDelta.Final = final;
                delta.Instanced.Add(itemDelta);
            }
            else if (instancedItemUpdate.Ext.SetViewed)
            {
                CustomizedInstancedItem item = await CustomizedInstancedItem.RetrieveFromDatabase(Guid.Parse(instancedItemUpdate.InstanceId));
                InstancedDelta itemDelta = new()
                {
                    Initial = ConvertCustomizedInstanced(item)
                };
                item.Viewed = true;
                await item.SyncToDatabase();
                Packets.InstancedItem final = ConvertCustomizedInstanced(item);
                segment.Instanced.Add(final);
                itemDelta.CatalogId = item.CatalogId;
                itemDelta.Operation = "UPDATED";
                itemDelta.Final = final;
                delta.Instanced.Add(itemDelta);
            }
        }
        foreach (StackedItemUpdate? stackableItemUpdate in req.StackableItemsUpdates)
        {
            Model.StackableItem item = await Model.StackableItem.RetrieveFromDatabase(Guid.Parse(stackableItemUpdate.InstanceId));
            Packets.StackableItem init = item.ToPacket();
            StackableDelta itemDelta = new()
            {
                Initial = init
            };
            item.Amount = long.Parse(stackableItemUpdate.NewAmount);
            await item.SyncToDatabase();
            Packets.StackableItem final = item.ToPacket();
            segment.Stackables.Add(final);
            itemDelta.CatalogId = item.CatalogId;
            itemDelta.Final = final;
            itemDelta.Operation = "UPDATED";
            delta.Stackables.Add(itemDelta);
        }
        var profileData = await Model.ProfileData.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        profileData.InventoryVersion++;
        await profileData.SyncToDatabase();
        // TODO: Send InventoryUpdated notification
        return SpectreWebsocketMessage.From(res);
    }
}