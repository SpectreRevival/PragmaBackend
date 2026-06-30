using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class OutfitLoadout : IDatabaseSyncableDefault<OutfitLoadout, Guid>, IEquatable<OutfitLoadout>, IInterchangeable<OutfitLoadout, Packets.OutfitLoadout>
{
    private static readonly OutfitLoadout defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "OutfitLoadout.json")))
        .Deserialize<OutfitLoadout>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

    [SetsRequiredMembers]
    public OutfitLoadout(Guid playerId, Guid loadoutId, OutfitData head, OutfitData hair, OutfitData faceStyle, OutfitData faceAccessory, OutfitData outfit)
    {
        PlayerId = playerId;
        LoadoutId = loadoutId;
        Head = head ?? throw new ArgumentNullException(nameof(head));
        Hair = hair ?? throw new ArgumentNullException(nameof(hair));
        FaceStyle = faceStyle ?? throw new ArgumentNullException(nameof(faceStyle));
        FaceAccessory = faceAccessory ?? throw new ArgumentNullException(nameof(faceAccessory));
        Outfit = outfit ?? throw new ArgumentNullException(nameof(outfit));
    }

    public required Guid PlayerId { get; set; }
    public required Guid LoadoutId { get; set; }
    public required OutfitData Head { get; set; }
    public required OutfitData Hair { get; set; }
    public required OutfitData FaceStyle { get; set; }
    public required OutfitData FaceAccessory { get; set; }
    public required OutfitData Outfit { get; set; }

    public static async Task<OutfitLoadout?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_outfit_loadout.sql");
        cmd.Parameters.AddWithValue("loadout_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new OutfitLoadout(
            await reader.GetFieldValueAsync<Guid>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<OutfitData>(2),
            await reader.GetFieldValueAsync<OutfitData>(3),
            await reader.GetFieldValueAsync<OutfitData>(4),
            await reader.GetFieldValueAsync<OutfitData>(5),
            await reader.GetFieldValueAsync<OutfitData>(6)
        );
    }

    public Guid GetKey()
    {
        return LoadoutId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_outfit_loadout.sql");
        cmd.Parameters.AddWithValue("loadout_id", LoadoutId);
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("head", Head);
        cmd.Parameters.AddWithValue("hair", Hair);
        cmd.Parameters.AddWithValue("face_style", FaceStyle);
        cmd.Parameters.AddWithValue("face_accessory", FaceAccessory);
        cmd.Parameters.AddWithValue("outfit", Outfit);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(OutfitLoadout? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (LoadoutId == other.LoadoutId
            && PlayerId == other.PlayerId
            && Head.Equals(other.Head)
            && Hair.Equals(other.Hair)
            && FaceStyle.Equals(other.FaceStyle)
            && FaceAccessory.Equals(other.FaceAccessory)
            && Outfit.Equals(other.Outfit)));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(LoadoutId);
        hash.Add(PlayerId);
        hash.Add(Hair);
        hash.Add(Head);
        hash.Add(FaceStyle);
        hash.Add(FaceAccessory);
        hash.Add(Outfit);
        return hash.ToHashCode();
    }

    public static OutfitLoadout CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId, LoadoutId = Guid.NewGuid() };
    }

    public static OutfitLoadout FromPacket(Packets.OutfitLoadout inst)
    {
        return new OutfitLoadout(Guid.Parse(inst.PlayerId), Guid.Parse(inst.LoadoutId), OutfitData.FromPacket(inst.HeadData), OutfitData.FromPacket(inst.HairData), OutfitData.FromPacket(inst.FaceStyleData), OutfitData.FromPacket(inst.FaceAccessoryData), OutfitData.FromPacket(inst.OutfitData));
    }

    public Packets.OutfitLoadout ToPacket()
    {
        return new Packets.OutfitLoadout()
        {
            PlayerId = PlayerId.ToString(),
            LoadoutId = LoadoutId.ToString(),
            HeadData = Head.ToPacket(),
            HairData = Hair.ToPacket(),
            FaceAccessoryData = FaceAccessory.ToPacket(),
            FaceStyleData = FaceStyle.ToPacket(),
            OutfitData = Outfit.ToPacket()
        };
    }
}