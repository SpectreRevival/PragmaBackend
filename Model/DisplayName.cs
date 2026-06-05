using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class DisplayName
{
    [SetsRequiredMembers]
    public DisplayName(string playerName, string discriminator)
    {
        PlayerName = playerName ?? throw new ArgumentNullException(nameof(playerName));
        Discriminator = discriminator ?? throw new ArgumentNullException(nameof(discriminator));
    }

    [PgName("player_name")]
    public required string PlayerName { get; set; }
    [PgName("discriminator")]
    public required string Discriminator { get; set; }
}