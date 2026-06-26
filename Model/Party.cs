using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class Party : VersionedData, IDatabaseSyncable<Party, Guid>, IEquatable<Party>
{
    [SetsRequiredMembers]
    public Party(Guid partyId, PartyMember[] members, string inviteCode, string queuePool, string lobbyMode, string chatId, bool useTeamMMR, Int64 version, string partyExtVersion = "173322", string region = "", string tag = "", string profile = "", bool hasAcceptableRegion = true, string[]? preferredGameServerZones = null, Dictionary<string, string>? standard = null, string customJson = "", string crossplayPlatform = "CROSS_PLAY_PLATFORM_PC") : base(version)
    {
        PartyId = partyId;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        InviteCode = inviteCode ?? throw new ArgumentNullException(nameof(inviteCode));
        QueuePool = queuePool ?? throw new ArgumentNullException(nameof(queuePool));
        LobbyMode = lobbyMode ?? throw new ArgumentNullException(nameof(lobbyMode));
        ChatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
        UseTeamMMR = useTeamMMR;
        PartyExtVersion = partyExtVersion ?? throw new ArgumentNullException(nameof(partyExtVersion));
        Region = region ?? throw new ArgumentNullException(nameof(region));
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        HasAcceptableRegion = hasAcceptableRegion;
        PreferredGameServerZones = preferredGameServerZones ?? ["uscentral-1"];
        Standard = standard ?? new Dictionary<string, string> { ["mode"] = "Standard" };
        CustomJson = customJson ?? throw new ArgumentNullException(nameof(customJson));
        CrossplayPlatform = crossplayPlatform ?? throw new ArgumentNullException(nameof(crossplayPlatform));
    }

    public required Guid PartyId { get; set; }
    public required PartyMember[] Members { get; set; }
    public required string InviteCode { get; set; }
    public required string QueuePool { get; set; }
    public required string LobbyMode { get; set; }
    public required string ChatId { get; set; }
    public required bool UseTeamMMR { get; set; }
    public required string PartyExtVersion { get; set; }
    public required string Region { get; set; }
    public required string Tag { get; set; }
    public required string Profile { get; set; }
    public required bool HasAcceptableRegion { get; set; }
    public required string[] PreferredGameServerZones { get; set; }
    public required Dictionary<string, string> Standard { get; set; }
    public required string CustomJson { get; set; }
    public required string CrossplayPlatform { get; set; }

    public static async Task<Party?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_party.sql");
        cmd.Parameters.AddWithValue("party_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new Party(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<PartyMember[]>(1),
            await reader.GetFieldValueAsync<string>(2),
            await reader.GetFieldValueAsync<string>(3),
            await reader.GetFieldValueAsync<string>(4),
            await reader.GetFieldValueAsync<string>(5),
            await reader.GetFieldValueAsync<bool>(6),
            await reader.GetFieldValueAsync<Int64>(7),
            await reader.GetFieldValueAsync<string>(8),
            await reader.GetFieldValueAsync<string>(9),
            await reader.GetFieldValueAsync<string>(10),
            await reader.GetFieldValueAsync<string>(11),
            await reader.GetFieldValueAsync<bool>(12),
            await reader.GetFieldValueAsync<string[]>(13),
            await reader.GetFieldValueAsync<Dictionary<string, string>>(14),
            await reader.GetFieldValueAsync<string>(15),
            await reader.GetFieldValueAsync<string>(16)
        );
    }

    public Guid GetKey()
    {
        return PartyId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_party.sql");
        cmd.Parameters.AddWithValue("party_id", PartyId);
        cmd.Parameters.AddWithValue("members", Members);
        cmd.Parameters.AddWithValue("invite_code", InviteCode);
        cmd.Parameters.AddWithValue("queue_pool", QueuePool);
        cmd.Parameters.AddWithValue("lobby_mode", LobbyMode);
        cmd.Parameters.AddWithValue("chat_id", ChatId);
        cmd.Parameters.AddWithValue("use_team_mmr", UseTeamMMR);
        cmd.Parameters.AddWithValue("version", Version);
        cmd.Parameters.AddWithValue("party_ext_version", PartyExtVersion);
        cmd.Parameters.AddWithValue("region", Region);
        cmd.Parameters.AddWithValue("tag", Tag);
        cmd.Parameters.AddWithValue("profile", Profile);
        cmd.Parameters.AddWithValue("has_acceptable_region", HasAcceptableRegion);
        cmd.Parameters.AddWithValue("preferred_game_server_zones", PreferredGameServerZones);
        cmd.Parameters.Add(new NpgsqlParameter("standard", NpgsqlTypes.NpgsqlDbType.Hstore) { Value = Standard });
        cmd.Parameters.AddWithValue("custom_json", CustomJson);
        cmd.Parameters.AddWithValue("crossplay_platform", CrossplayPlatform);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(Party? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PartyId == other.PartyId
            && Members.SequenceEqual(other.Members)
            && InviteCode == other.InviteCode
            && QueuePool == other.QueuePool
            && LobbyMode == other.LobbyMode
            && ChatId == other.ChatId
            && UseTeamMMR == other.UseTeamMMR
            && PartyExtVersion == other.PartyExtVersion
            && Region == other.Region
            && Tag == other.Tag
            && Profile == other.Profile
            && HasAcceptableRegion == other.HasAcceptableRegion
            && PreferredGameServerZones.SequenceEqual(other.PreferredGameServerZones)
            && Standard.Count == other.Standard.Count
            && !Standard.Except(other.Standard).Any()
            && CustomJson == other.CustomJson
            && CrossplayPlatform == other.CrossplayPlatform
            && Version == other.Version;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PartyId);
        hash.Add(Members);
        hash.Add(InviteCode);
        hash.Add(QueuePool);
        hash.Add(LobbyMode);
        hash.Add(ChatId);
        hash.Add(UseTeamMMR);
        hash.Add(PartyExtVersion);
        hash.Add(Region);
        hash.Add(Tag);
        hash.Add(Profile);
        hash.Add(HasAcceptableRegion);
        hash.Add(PreferredGameServerZones);
        hash.Add(Standard);
        hash.Add(CustomJson);
        hash.Add(CrossplayPlatform);
        hash.Add(Version);
        return hash.ToHashCode();
    }
}