using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ClientMessage : IDatabaseSyncable<ClientMessage, Guid>, IEquatable<ClientMessage>, IInterchangeable<ClientMessage, Packets.ClientMessage>
{
    [SetsRequiredMembers]
    public ClientMessage(Guid messageId, Guid playerId, string messageType, string messageSender, string messageSenderType, string campaignId, string messageTitle, string messageBody, string? itemAttachmentCatalogId, DateTimeOffset sentTime, DateTimeOffset readTime, DateTimeOffset expirationTime)
    {
        MessageId = messageId;
        PlayerId = playerId;
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        MessageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        MessageSenderType = messageSenderType ?? throw new ArgumentNullException(nameof(messageSenderType));
        CampaignId = campaignId ?? throw new ArgumentNullException(nameof(campaignId));
        MessageTitle = messageTitle ?? throw new ArgumentNullException(nameof(messageTitle));
        MessageBody = messageBody ?? throw new ArgumentNullException(nameof(messageBody));
        ItemAttachmentCatalogId = itemAttachmentCatalogId;
        SentTime = sentTime;
        ReadTime = readTime;
        ExpirationTime = expirationTime;
    }

    public required Guid MessageId { get; set; }
    public required Guid PlayerId { get; set; }
    public required string MessageType { get; set; }
    public required string MessageSender { get; set; }
    public required string MessageSenderType { get; set; }
    public required string CampaignId { get; set; }
    public required string MessageTitle { get; set; }
    public required string MessageBody { get; set; }
    public required string? ItemAttachmentCatalogId { get; set; }
    public required DateTimeOffset SentTime { get; set; }
    public required DateTimeOffset ReadTime { get; set; }
    public required DateTimeOffset ExpirationTime { get; set; }

    public static async Task<ClientMessage?> RetrieveFromDatabase(Guid messageId)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_client_message.sql");
        cmd.Parameters.AddWithValue("message_id", messageId);
        using var reader = await cmd.ExecuteReaderAsync();
        if(!await reader.ReadAsync()){
            return null;
        }
        return new ClientMessage(
             await reader.GetFieldValueAsync<Guid>(0),
             await reader.GetFieldValueAsync<Guid>(1),
             await reader.GetFieldValueAsync<string>(2),
             await reader.GetFieldValueAsync<string>(3),
             await reader.GetFieldValueAsync<string>(4),
             await reader.GetFieldValueAsync<string>(5),
             await reader.GetFieldValueAsync<string>(6),
             await reader.GetFieldValueAsync<string>(7),
             await reader.IsDBNullAsync(8) ? null : await reader.GetFieldValueAsync<string>(8),
             await reader.GetFieldValueAsync<DateTimeOffset>(9),
             await reader.GetFieldValueAsync<DateTimeOffset>(10),
             await reader.GetFieldValueAsync<DateTimeOffset>(11)
        );
    }

    public Guid GetKey()
    {
        return MessageId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_client_message.sql");
        cmd.Parameters.AddWithValue("message_id", MessageId);
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("message_type", MessageType);
        cmd.Parameters.AddWithValue("message_sender", MessageSender);
        cmd.Parameters.AddWithValue("message_sender_type", MessageSenderType);
        cmd.Parameters.AddWithValue("campaign_id", CampaignId);
        cmd.Parameters.AddWithValue("message_title", MessageTitle);
        cmd.Parameters.AddWithValue("message_body", MessageBody);
        cmd.Parameters.AddWithValue("item_attachment_catalog_id", ItemAttachmentCatalogId);
        cmd.Parameters.AddWithValue("sent_time", SentTime);
        cmd.Parameters.AddWithValue("read_time", ReadTime);
        cmd.Parameters.AddWithValue("expiration_time", ExpirationTime);
        await cmd.ExecuteNonQueryAsync();
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MessageId);
        hash.Add(PlayerId);
        hash.Add(MessageType);
        hash.Add(MessageSender);
        hash.Add(MessageSenderType);
        hash.Add(CampaignId);
        hash.Add(MessageTitle);
        hash.Add(MessageBody);
        hash.Add(ItemAttachmentCatalogId);
        hash.Add(SentTime.ToUnixTimeMilliseconds());
        hash.Add(ReadTime.ToUnixTimeMilliseconds());
        hash.Add(ExpirationTime.ToUnixTimeMilliseconds());
        return hash.ToHashCode();
    }

    public virtual bool Equals(ClientMessage? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MessageId == other.MessageId && PlayerId == other.PlayerId
            && MessageType == other.MessageType && MessageSender == other.MessageSender
            && MessageSenderType == other.MessageSenderType && CampaignId == other.CampaignId
            && MessageTitle == other.MessageTitle && MessageBody == other.MessageBody
            && ItemAttachmentCatalogId == other.ItemAttachmentCatalogId && SentTime.ToUnixTimeMilliseconds() == other.SentTime.ToUnixTimeMilliseconds()
            && ReadTime.ToUnixTimeMilliseconds() == other.ReadTime.ToUnixTimeMilliseconds() && ExpirationTime.ToUnixTimeMilliseconds() == other.ExpirationTime.ToUnixTimeMilliseconds();
    }

    public static ClientMessage FromPacket(Packets.ClientMessage inst)
    {
        return new ClientMessage(Guid.Parse(inst.MessageId), Guid.Parse(inst.MessageData.PlayerId), inst.MessageData.MessageType, inst.MessageData.SentBy.Value, inst.MessageData.SentBy.Type,
            inst.MessageData.CampaignId, inst.MessageData.MessageTitle, inst.MessageData.MessageBody, inst.MessageData.InstancedItemGrantAttachment != null ? inst.MessageData.InstancedItemGrantAttachment.CatalogId : null,
            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.MessageData.SentTimestamp)),
            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.MessageData.ReadTimestamp)),
            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.MessageData.ExpirationTimestamp)));
    }

    public Packets.ClientMessage ToPacket()
    {
        var packet = new Packets.ClientMessage()
        {
            MessageId = MessageId.ToString()
        };
        var msgData = new Packets.MessageData()
        {
            PlayerId = PlayerId.ToString(),
            MessageType = MessageType,
            MessageTitle = MessageTitle,
            CampaignId = CampaignId,
            MessageBody = MessageBody,
            SentDate = SentTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            SentTimestamp = SentTime.ToUnixTimeMilliseconds().ToString(),
            ReadDate = ReadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            ReadTimestamp = ReadTime.ToUnixTimeMilliseconds().ToString(),
            ExpirationDate = ExpirationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            ExpirationTimestamp = ExpirationTime.ToUnixTimeMilliseconds().ToString()
        };
        var sender = new MessageSender()
        {
            Type = MessageSenderType,
            Value = MessageSender
        };
        msgData.SentBy = sender;
        if(ItemAttachmentCatalogId != null)
        {
            var itemAttachment = new MessageItemGrant()
            {
                CatalogId = ItemAttachmentCatalogId,
                InstanceId = "00000000-0000-0000-0000-000000000000"
            };
            msgData.InstancedItemGrantAttachment = itemAttachment;
        }
        packet.MessageData = msgData;
        return packet;
    }
}