using NpgsqlTypes;

namespace Model;

public record class ActiveAlterationData
{
    [PgName("channel_id")]
    public required string ChannelId { get; set; }
    [PgName("alteration_id")]
    public required string AlterationId { get; set; }
}