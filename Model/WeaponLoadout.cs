using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class WeaponLoadout : IDatabaseSyncableDefault<WeaponLoadout, Guid>, IEquatable<WeaponLoadout>, IInterchangeable<WeaponLoadout, Packets.WeaponLoadout>
{
    private static readonly WeaponLoadout defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "WeaponLoadout.json")))
        .Deserialize<WeaponLoadout>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

    [SetsRequiredMembers]
    public WeaponLoadout(Guid playerId, Guid loadoutId, WeaponData semiAutoPistol, WeaponData suppressedPistol, WeaponData autoPistol, WeaponData highcalPistol, WeaponData heavyShotgun, WeaponData autoShotgun, WeaponData tacticalSMG, WeaponData rapidfireSMG, WeaponData suppressedSMG, WeaponData standardAR, WeaponData semiAutoAR, WeaponData burstAR, WeaponData tacticalAR, WeaponData suppressedAR, WeaponData heavyAR, WeaponData highcalMG, WeaponData rapidfireMG, WeaponData semiAutoSniper, WeaponData boltActionSniper, WeaponData melee)
    {
        PlayerId = playerId;
        LoadoutId = loadoutId;
        SemiAutoPistol = semiAutoPistol ?? throw new ArgumentNullException(nameof(semiAutoPistol));
        SuppressedPistol = suppressedPistol ?? throw new ArgumentNullException(nameof(suppressedPistol));
        AutoPistol = autoPistol ?? throw new ArgumentNullException(nameof(autoPistol));
        HighcalPistol = highcalPistol ?? throw new ArgumentNullException(nameof(highcalPistol));
        HeavyShotgun = heavyShotgun ?? throw new ArgumentNullException(nameof(heavyShotgun));
        AutoShotgun = autoShotgun ?? throw new ArgumentNullException(nameof(autoShotgun));
        TacticalSMG = tacticalSMG ?? throw new ArgumentNullException(nameof(tacticalSMG));
        RapidfireSMG = rapidfireSMG ?? throw new ArgumentNullException(nameof(rapidfireSMG));
        SuppressedSMG = suppressedSMG ?? throw new ArgumentNullException(nameof(suppressedSMG));
        StandardAR = standardAR ?? throw new ArgumentNullException(nameof(standardAR));
        SemiAutoAR = semiAutoAR ?? throw new ArgumentNullException(nameof(semiAutoAR));
        BurstAR = burstAR ?? throw new ArgumentNullException(nameof(burstAR));
        TacticalAR = tacticalAR ?? throw new ArgumentNullException(nameof(tacticalAR));
        SuppressedAR = suppressedAR ?? throw new ArgumentNullException(nameof(suppressedAR));
        HeavyAR = heavyAR ?? throw new ArgumentNullException(nameof(heavyAR));
        HighcalMG = highcalMG ?? throw new ArgumentNullException(nameof(highcalMG));
        RapidfireMG = rapidfireMG ?? throw new ArgumentNullException(nameof(rapidfireMG));
        SemiAutoSniper = semiAutoSniper ?? throw new ArgumentNullException(nameof(semiAutoSniper));
        BoltActionSniper = boltActionSniper ?? throw new ArgumentNullException(nameof(boltActionSniper));
        Melee = melee ?? throw new ArgumentNullException(nameof(melee));
    }

    public required Guid PlayerId { get; set; }
    public required Guid LoadoutId { get; set; }
    public required WeaponData SemiAutoPistol { get; set; }
    public required WeaponData SuppressedPistol { get; set; }
    public required WeaponData AutoPistol { get; set; }
    public required WeaponData HighcalPistol { get; set; }
    public required WeaponData HeavyShotgun { get; set; }
    public required WeaponData AutoShotgun { get; set; }
    public required WeaponData TacticalSMG { get; set; }
    public required WeaponData RapidfireSMG { get; set; }
    public required WeaponData SuppressedSMG { get; set; }
    public required WeaponData StandardAR { get; set; }
    public required WeaponData SemiAutoAR { get; set; }
    public required WeaponData BurstAR { get; set; }
    public required WeaponData TacticalAR { get; set; }
    public required WeaponData SuppressedAR { get; set; }
    public required WeaponData HeavyAR { get; set; }
    public required WeaponData HighcalMG { get; set; }
    public required WeaponData RapidfireMG { get; set; }
    public required WeaponData SemiAutoSniper { get; set; }
    public required WeaponData BoltActionSniper { get; set; }
    public required WeaponData Melee { get; set; }

    public static async Task<WeaponLoadout?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_weapon_loadout.sql");
        cmd.Parameters.AddWithValue("loadout_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new WeaponLoadout(
            await reader.GetFieldValueAsync<Guid>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<WeaponData>(2),
            await reader.GetFieldValueAsync<WeaponData>(3),
            await reader.GetFieldValueAsync<WeaponData>(4),
            await reader.GetFieldValueAsync<WeaponData>(5),
            await reader.GetFieldValueAsync<WeaponData>(6),
            await reader.GetFieldValueAsync<WeaponData>(7),
            await reader.GetFieldValueAsync<WeaponData>(8),
            await reader.GetFieldValueAsync<WeaponData>(9),
            await reader.GetFieldValueAsync<WeaponData>(10),
            await reader.GetFieldValueAsync<WeaponData>(11),
            await reader.GetFieldValueAsync<WeaponData>(12),
            await reader.GetFieldValueAsync<WeaponData>(13),
            await reader.GetFieldValueAsync<WeaponData>(14),
            await reader.GetFieldValueAsync<WeaponData>(15),
            await reader.GetFieldValueAsync<WeaponData>(16),
            await reader.GetFieldValueAsync<WeaponData>(17),
            await reader.GetFieldValueAsync<WeaponData>(18),
            await reader.GetFieldValueAsync<WeaponData>(19),
            await reader.GetFieldValueAsync<WeaponData>(20),
            await reader.GetFieldValueAsync<WeaponData>(21)
        );
    }

    public Guid GetKey()
    {
        return LoadoutId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_weapon_loadout.sql");
        cmd.Parameters.AddWithValue("loadout_id", LoadoutId);
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("semi_auto_pistol", SemiAutoPistol);
        cmd.Parameters.AddWithValue("suppressed_pistol", SuppressedPistol);
        cmd.Parameters.AddWithValue("auto_pistol", AutoPistol);
        cmd.Parameters.AddWithValue("highcal_pistol", HighcalPistol);
        cmd.Parameters.AddWithValue("heavy_shotgun", HeavyShotgun);
        cmd.Parameters.AddWithValue("auto_shotgun", AutoShotgun);
        cmd.Parameters.AddWithValue("tactical_smg", TacticalSMG);
        cmd.Parameters.AddWithValue("rapidfire_smg", RapidfireSMG);
        cmd.Parameters.AddWithValue("suppressed_smg", SuppressedSMG);
        cmd.Parameters.AddWithValue("standard_ar", StandardAR);
        cmd.Parameters.AddWithValue("semi_auto_ar", SemiAutoAR);
        cmd.Parameters.AddWithValue("burst_ar", BurstAR);
        cmd.Parameters.AddWithValue("tactical_ar", TacticalAR);
        cmd.Parameters.AddWithValue("suppressed_ar", SuppressedAR);
        cmd.Parameters.AddWithValue("heavy_ar", HeavyAR);
        cmd.Parameters.AddWithValue("highcal_mg", HighcalMG);
        cmd.Parameters.AddWithValue("rapidfire_mg", RapidfireMG);
        cmd.Parameters.AddWithValue("semi_auto_sniper", SemiAutoSniper);
        cmd.Parameters.AddWithValue("bolt_action_sniper", BoltActionSniper);
        cmd.Parameters.AddWithValue("melee", Melee);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(WeaponLoadout? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (LoadoutId == other.LoadoutId
            && PlayerId == other.PlayerId
            && SemiAutoPistol.Equals(other.SemiAutoPistol)
            && SuppressedPistol.Equals(other.SuppressedPistol)
            && AutoPistol.Equals(other.AutoPistol)
            && HighcalPistol.Equals(other.HighcalPistol)
            && HeavyShotgun.Equals(other.HeavyShotgun)
            && AutoShotgun.Equals(other.AutoShotgun)
            && TacticalSMG.Equals(other.TacticalSMG)
            && RapidfireSMG.Equals(other.RapidfireSMG)
            && SuppressedSMG.Equals(other.SuppressedSMG)
            && StandardAR.Equals(other.StandardAR)
            && SemiAutoAR.Equals(other.SemiAutoAR)
            && BurstAR.Equals(other.BurstAR)
            && TacticalAR.Equals(other.TacticalAR)
            && SuppressedAR.Equals(other.SuppressedAR)
            && HeavyAR.Equals(other.HeavyAR)
            && HighcalMG.Equals(other.HighcalMG)
            && RapidfireMG.Equals(other.RapidfireMG)
            && SemiAutoSniper.Equals(other.SemiAutoSniper)
            && BoltActionSniper.Equals(other.BoltActionSniper)
            && Melee.Equals(other.Melee)));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(LoadoutId);
        hash.Add(PlayerId);
        hash.Add(SemiAutoPistol);
        hash.Add(SuppressedPistol);
        hash.Add(AutoPistol);
        hash.Add(HighcalPistol);
        hash.Add(HeavyShotgun);
        hash.Add(AutoShotgun);
        hash.Add(TacticalSMG);
        hash.Add(StandardAR);
        hash.Add(SemiAutoAR);
        hash.Add(BurstAR);
        hash.Add(TacticalAR);
        hash.Add(SuppressedAR);
        hash.Add(HeavyAR);
        hash.Add(HighcalMG);
        hash.Add(RapidfireMG);
        hash.Add(SemiAutoSniper);
        hash.Add(BoltActionSniper);
        hash.Add(Melee);
        return hash.ToHashCode();
    }

    public static WeaponLoadout CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId, LoadoutId = Guid.NewGuid() };
    }

    public static WeaponLoadout FromPacket(Packets.WeaponLoadout inst)
    {
        return new WeaponLoadout(Guid.Parse(inst.PlayerId), Guid.Parse(inst.LoadoutId), WeaponData.FromPacket(inst.SemiautoPistolData), WeaponData.FromPacket(inst.SuppressedPistolData), WeaponData.FromPacket(inst.AutoPistolData), WeaponData.FromPacket(inst.HighcalPistolData), WeaponData.FromPacket(inst.HeavyShotgunData), WeaponData.FromPacket(inst.AutoShotgunData), WeaponData.FromPacket(inst.TacticalSmgData), WeaponData.FromPacket(inst.RapidfireSmgData), WeaponData.FromPacket(inst.SuppressedSmgData), WeaponData.FromPacket(inst.StandardArData), WeaponData.FromPacket(inst.SemiautoArData), WeaponData.FromPacket(inst.BurstArData), WeaponData.FromPacket(inst.TacticalArData), WeaponData.FromPacket(inst.SuppressedArData), WeaponData.FromPacket(inst.HeavyArData), WeaponData.FromPacket(inst.HighcalMgData), WeaponData.FromPacket(inst.RapidfireMgData), WeaponData.FromPacket(inst.SemiautoSniperData), WeaponData.FromPacket(inst.BoltactionSniperData), WeaponData.FromPacket(inst.MeleeData));
    }

    public Packets.WeaponLoadout ToPacket()
    {
        return new Packets.WeaponLoadout()
        {
            LoadoutId = LoadoutId.ToString(),
            PlayerId = PlayerId.ToString(),
            SemiautoPistolData = SemiAutoPistol.ToPacket(),
            SuppressedPistolData = SuppressedPistol.ToPacket(),
            AutoPistolData = AutoPistol.ToPacket(),
            HighcalPistolData = HighcalPistol.ToPacket(),
            HeavyShotgunData = HeavyShotgun.ToPacket(),
            AutoShotgunData = AutoShotgun.ToPacket(),
            TacticalSmgData = TacticalSMG.ToPacket(),
            RapidfireSmgData = RapidfireSMG.ToPacket(),
            SuppressedSmgData = SuppressedSMG.ToPacket(),
            StandardArData = StandardAR.ToPacket(),
            SemiautoArData = SemiAutoAR.ToPacket(),
            BurstArData = BurstAR.ToPacket(),
            TacticalArData = TacticalAR.ToPacket(),
            SuppressedArData = SuppressedAR.ToPacket(),
            HeavyArData = HeavyAR.ToPacket(),
            HighcalMgData = HighcalMG.ToPacket(),
            RapidfireMgData = RapidfireMG.ToPacket(),
            SemiautoSniperData = SemiAutoSniper.ToPacket(),
            BoltactionSniperData = BoltActionSniper.ToPacket(),
            MeleeData = Melee.ToPacket(),
        };
    }
}