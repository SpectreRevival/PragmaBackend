using Packets;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class LeavePartyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public LeavePartyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.LeaveV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        Log.Information("PARTYDIAG: LeaveV1 from {PlayerId}", ConnectionHandler.PlayerId);
        await RemovePlayerFromParties(ConnectionHandler.PlayerId, null);
        return SpectreWebsocketMessage.Empty();
    }
}

public class UpdatePartyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public UpdatePartyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.UpdatePartyV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        UpdatePartyRequest request = Packet.GetPayloadAsMessage<UpdatePartyRequest>();
        Log.Information("PARTYDIAG: UpdatePartyV1 from {PlayerId} raw={Raw}", ConnectionHandler.PlayerId, Packet.RequestPayload.ToJsonString());
        Model.Party party = await WithPartyLock(Guid.Parse(request.PartyId), async () =>
        {
            Model.Party locked = await GetPartyOrThrow(request.PartyId);
            ApplyPartyUpdate(locked, request.RequestExt ?? new PartyUpdate());
            await locked.SyncToDatabase();
            return locked;
        });
        return await CreatePartyMessageWithMemberFanout(party, ConnectionHandler.PlayerId);
    }
}

public class UpdatePartyPlayerProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public UpdatePartyPlayerProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.UpdatePartyPlayerV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        UpdatePartyPlayerRequest request = Packet.GetPayloadAsMessage<UpdatePartyPlayerRequest>();
        Log.Information("PARTYDIAG: UpdatePartyPlayerV1 from {PlayerId} raw={Raw}", ConnectionHandler.PlayerId, Packet.RequestPayload.ToJsonString());
        PartyPlayerUpdateData ext = request.RequestExt ?? new PartyPlayerUpdateData();
        if (ext.RefreshData)
        {
            Model.Party current = await GetPartyOrThrow(request.PartyId);
            return await CreatePartyMessage(current);
        }

        Model.Party party = await WithPartyLock(Guid.Parse(request.PartyId), async () =>
        {
            Model.Party locked = await GetPartyOrThrow(request.PartyId);
            ApplyPartyPlayerUpdate(locked, ConnectionHandler.PlayerId, ext);
            await locked.SyncToDatabase();
            return locked;
        });
        return await CreatePartyMessageWithMemberFanout(party, ConnectionHandler.PlayerId);
    }
}

public class SetReadyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SetReadyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.SetReadyStateV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        SetReadyMessage request = Packet.GetPayloadAsMessage<SetReadyMessage>();
        Model.Party party = await WithPartyLock(Guid.Parse(request.PartyId), async () =>
        {
            Model.Party locked = await GetPartyOrThrow(request.PartyId);
            ApplyReadyState(locked, ConnectionHandler.PlayerId, request.Ready);
            await locked.SyncToDatabase();
            return locked;
        });
        return await CreatePartyMessageWithMemberFanout(party, ConnectionHandler.PlayerId);
    }
}

public class EnterMatchmakingProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public EnterMatchmakingProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.EnterMatchmakingV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        EnterMatchmakingRequest request = Packet.GetPayloadAsMessage<EnterMatchmakingRequest>();
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        WebsocketNotification[] postNotifs = await QueueMatchmakingNotifications(party, ConnectionHandler);
        return await CreatePartyMessage(party, postNotifs);
    }
}

public class KickPartyPlayerProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public KickPartyPlayerProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.KickV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        string? targetIdString = ReadStringField(Packet.RequestPayload, "playerId", "player_id");
        string? partyIdString = ReadStringField(Packet.RequestPayload, "partyId", "party_id");
        if (string.IsNullOrEmpty(targetIdString) || string.IsNullOrEmpty(partyIdString))
        {
            throw new InvalidDataException("Kick request missing playerId or partyId");
        }

        Guid targetId = Guid.Parse(targetIdString);
        Model.Party party = await GetPartyOrThrow(partyIdString);
        Model.PartyMember? requester = Array.Find(party.Members, m => m.PlayerId == ConnectionHandler.PlayerId);
        if (requester is null || !requester.IsLeader)
        {
            throw new InvalidDataException($"Kick requested by non-leader {ConnectionHandler.PlayerId}");
        }
        if (targetId == ConnectionHandler.PlayerId)
        {
            throw new InvalidDataException("Kick target is the leader, use LeaveV1");
        }

        Log.Information("PARTYDIAG: KickV1 leader {LeaderId} kicking {TargetId} from {PartyId}", ConnectionHandler.PlayerId, targetId, partyIdString);
        await RemovePlayerFromParties(targetId, null);

        JsonObject removedNotif = new()
        {
            ["partyId"] = partyIdString,
            ["removalReason"] = "KICKED"
        };
        await SpectreWebsocket.SendNotificationToPlayerAsync(targetId,
            new SpectreRpcType("PartyRpc.RemovedV1Notification"), removedNotif.ToJsonString());

        Model.Party updated = await GetPartyOrThrow(partyIdString);
        return await CreatePartyMessage(updated);
    }
}