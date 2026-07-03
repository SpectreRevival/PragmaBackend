using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class SendInviteProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SendInviteProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.SendInviteV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        string? partyIdString = ReadStringField(Packet.RequestPayload, "partyId", "party_id");
        string? inviteeString = ReadStringField(Packet.RequestPayload, "inviteePlayerId", "invitee_player_id",
            "playerId", "player_id");
        if (string.IsNullOrEmpty(partyIdString) || string.IsNullOrEmpty(inviteeString))
        {
            throw new InvalidDataException("SendInvite request missing party id or invitee player id");
        }

        Guid partyId = Guid.Parse(partyIdString);
        Guid inviteeId = Guid.Parse(inviteeString);
        Model.Party party = await GetPartyOrThrow(partyIdString);
        if (!Array.Exists(party.Members, m => m.PlayerId == ConnectionHandler.PlayerId))
        {
            throw new InvalidDataException($"Player {ConnectionHandler.PlayerId} is not in party {partyId}");
        }

        Guid inviteId = Guid.NewGuid();
        string blob = "";
        if (Packet.RequestPayload["extPartyInviteData"] is JsonObject extData)
        {
            blob = ReadStringField(extData, "blob") ?? "";
        }

        PendingInvites[inviteId] =
            new PendingPartyInvite(inviteId, partyId, ConnectionHandler.PlayerId, inviteeId, blob);
        JsonObject response = new() { ["inviteId"] = inviteId.ToString() };

        JsonObject notification = new()
        {
            ["inviteId"] = inviteId.ToString(),
            ["partyId"] = partyId.ToString(),
            ["inviterInfo"] = await CreatePlayerInfoJson(ConnectionHandler.PlayerId),
            ["extPartyInviteData"] = new JsonObject { ["blob"] = blob }
        };

        return SpectreWebsocketMessage.From(response, postResponseActions:
        [
            async () => await SpectreWebsocket.SendNotificationToPlayerAsync(
                inviteeId,
                InviteReceivedNotificationType,
                notification.ToJsonString())
        ]);
    }
}

public class RespondToInviteProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public RespondToInviteProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.RespondToInviteV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        string? inviteIdString = ReadStringField(Packet.RequestPayload, "inviteId", "invite_id");
        if (string.IsNullOrEmpty(inviteIdString))
        {
            throw new InvalidDataException("RespondToInvite request had no invite id");
        }

        Guid inviteId = Guid.Parse(inviteIdString);
        bool accepted = ReadBoolField(Packet.RequestPayload, false, "accepted", "accept", "bAccept");
        if (!PendingInvites.TryRemove(inviteId, out PendingPartyInvite? invite))
        {
            throw new InvalidDataException($"No pending party invite found for id {inviteId}");
        }

        if (invite.InviteePlayerId != ConnectionHandler.PlayerId)
        {
            throw new InvalidDataException($"Player {ConnectionHandler.PlayerId} cannot respond to invite {inviteId}");
        }

        JsonObject responseNotification = new()
        {
            ["inviteId"] = inviteId.ToString(),
            ["accepted"] = accepted,
            ["inviteeInfo"] = await CreatePlayerInfoJson(ConnectionHandler.PlayerId)
        };

        if (!accepted)
        {
            return SpectreWebsocketMessage.From(new JsonObject(), postResponseActions:
            [
                async () => await SpectreWebsocket.SendNotificationToPlayerAsync(
                    invite.InviterPlayerId,
                    InviteResponseNotificationType,
                    responseNotification.ToJsonString())
            ]);
        }

        Model.Party party = await JoinPartyById(invite.PartyId, ConnectionHandler.PlayerId,
            ReadJoinExt(Packet.RequestPayload));
        string partyJson = await BuildPartyJson(party);
        return SpectreWebsocketMessage.From(partyJson, postResponseActions:
        [
            async () => await SpectreWebsocket.SendNotificationToPlayerAsync(
                invite.InviterPlayerId,
                InviteResponseNotificationType,
                responseNotification.ToJsonString()),
            async () => await NotifyOtherMembersBestEffort(party, ConnectionHandler.PlayerId, partyJson)
        ]);
    }
}