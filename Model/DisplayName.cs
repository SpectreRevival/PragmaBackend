using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class DisplayName : IEquatable<DisplayName>, IInterchangeable<DisplayName, Packets.DisplayName>
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

    public static DisplayName FromPacket(Packets.DisplayName inst)
    {
        return new DisplayName(inst.DisplayName_, inst.Discriminator);
    }

    public Packets.DisplayName ToPacket()
    {
        return new Packets.DisplayName()
        {
            DisplayName_ = PlayerName,
            Discriminator = Discriminator
        };
    }

    public virtual bool Equals(DisplayName? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerName == other.PlayerName && Discriminator == other.Discriminator));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerName);
        hash.Add(Discriminator);
        return hash.ToHashCode();
    }
}