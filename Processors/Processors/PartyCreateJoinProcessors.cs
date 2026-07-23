using Packets;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class CreatePartyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public CreatePartyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.CreateV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        Log.Information("PARTYDIAG: CreateV1 from {PlayerId}", ConnectionHandler.PlayerId);
        await RemovePlayerFromParties(ConnectionHandler.PlayerId, null);
        Model.Party party =
            await CreateParty(Packet.GetPayloadAsMessage<CreatePartyRequest>(), ConnectionHandler.PlayerId);
        return await CreatePartyMessage(party);
    }
}

public class JoinWithInviteCodeProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public JoinWithInviteCodeProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.JoinWithInviteCodeV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        string? inviteCode = ReadStringField(Packet.RequestPayload, "inviteCode", "invite_code", "code");
        if (string.IsNullOrEmpty(inviteCode))
        {
            throw new InvalidDataException("JoinWithInviteCode request had no invite code");
        }

        Guid? partyId = await Model.Party.FindPartyIdByInviteCode(inviteCode);
        if (partyId is null)
        {
            throw new InvalidDataException($"No party found for invite code {inviteCode}");
        }

        Model.Party party =
            await JoinPartyById(partyId.Value, ConnectionHandler.PlayerId, ReadJoinExt(Packet.RequestPayload));
        return await CreatePartyMessageWithMemberFanout(party, ConnectionHandler.PlayerId);
    }
}

public class JoinWithPartyIdProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public JoinWithPartyIdProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.JoinWithPartyIdV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        string? partyId = ReadStringField(Packet.RequestPayload, "partyId", "party_id");
        if (string.IsNullOrEmpty(partyId))
        {
            throw new InvalidDataException("JoinWithPartyId request had no party id");
        }

        Model.Party party = await JoinPartyById(Guid.Parse(partyId), ConnectionHandler.PlayerId,
            ReadJoinExt(Packet.RequestPayload));
        return await CreatePartyMessageWithMemberFanout(party, ConnectionHandler.PlayerId);
    }
}