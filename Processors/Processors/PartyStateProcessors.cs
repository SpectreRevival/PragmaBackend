using Packets;
using System.Diagnostics.CodeAnalysis;

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
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        ApplyPartyUpdate(party, request.RequestExt ?? new PartyUpdate());
        await party.SyncToDatabase();
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
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        ApplyPartyPlayerUpdate(party, ConnectionHandler.PlayerId, request.RequestExt ?? new PartyPlayerUpdateData());
        await party.SyncToDatabase();
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
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        ApplyReadyState(party, ConnectionHandler.PlayerId, request.Ready);
        await party.SyncToDatabase();
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