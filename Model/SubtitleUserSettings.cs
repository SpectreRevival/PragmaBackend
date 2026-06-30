using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class SubtitleUserSettings : VersionedData, IDatabaseSyncableDefault<SubtitleUserSettings, Guid>, IEquatable<SubtitleUserSettings>, IInterchangeableKeyed<SubtitleUserSettings, Packets.SubtitleUserSettings, Guid>
{
    private static readonly SubtitleUserSettings defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "SubtitleUserSettings.json")))
        .Deserialize<SubtitleUserSettings>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

    [SetsRequiredMembers]
    public SubtitleUserSettings(Guid playerId, int fontSize, double backgroundOpacity, string speakerQualifierDisplay, bool postPlayerSubtitles, bool postPlayerSubtitlesToChat, int namesToShowMask, long version) : base(version)
    {
        PlayerId = playerId;
        FontSize = fontSize;
        BackgroundOpacity = backgroundOpacity;
        SpeakerQualifierDisplay = speakerQualifierDisplay ?? throw new ArgumentNullException(nameof(speakerQualifierDisplay));
        PostPlayerSubtitles = postPlayerSubtitles;
        PostPlayerSubtitlesToChat = postPlayerSubtitlesToChat;
        NamesToShowMask = namesToShowMask;
    }

    public required Guid PlayerId { get; set; }
    public required int FontSize { get; set; }
    public required double BackgroundOpacity { get; set; }
    public required string SpeakerQualifierDisplay { get; set; }
    public required bool PostPlayerSubtitles { get; set; }
    public required bool PostPlayerSubtitlesToChat { get; set; }
    public required int NamesToShowMask { get; set; }

    public static async Task<SubtitleUserSettings?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_subtitle_settings.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new SubtitleUserSettings(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<int>(1),
            await reader.GetFieldValueAsync<double>(2),
            await reader.GetFieldValueAsync<string>(3),
            await reader.GetFieldValueAsync<bool>(4),
            await reader.GetFieldValueAsync<bool>(5),
            await reader.GetFieldValueAsync<int>(6),
            await reader.GetFieldValueAsync<long>(7)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_subtitle_user_settings.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("font_size", FontSize);
        cmd.Parameters.AddWithValue("background_opacity", BackgroundOpacity);
        cmd.Parameters.AddWithValue("speaker_qualifier_display", SpeakerQualifierDisplay);
        cmd.Parameters.AddWithValue("post_player_subtitles", PostPlayerSubtitles);
        cmd.Parameters.AddWithValue("post_player_subtitles_to_chat", PostPlayerSubtitlesToChat);
        cmd.Parameters.AddWithValue("names_to_show_mask", NamesToShowMask);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(SubtitleUserSettings? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && FontSize == other.FontSize
            && BackgroundOpacity == other.BackgroundOpacity
            && SpeakerQualifierDisplay == other.SpeakerQualifierDisplay
            && PostPlayerSubtitles == other.PostPlayerSubtitles
            && PostPlayerSubtitlesToChat == other.PostPlayerSubtitlesToChat
            && NamesToShowMask == other.NamesToShowMask
            && Version == other.Version));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(FontSize);
        hash.Add(BackgroundOpacity);
        hash.Add(SpeakerQualifierDisplay);
        hash.Add(PostPlayerSubtitles);
        hash.Add(PostPlayerSubtitlesToChat);
        hash.Add(NamesToShowMask);
        hash.Add(Version);
        return hash.ToHashCode();
    }

    public static SubtitleUserSettings CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId };
    }

    public static SubtitleUserSettings FromPacket(Packets.SubtitleUserSettings inst, Guid id)
    {
        return new SubtitleUserSettings(id, inst.FontSize, inst.BackgroundOpacity, inst.SpeakerQualifierDisplay, inst.BPostPlayerSubtitles, inst.BPostPlayerSubtitlesToChat, inst.NamesToShowMask, inst.Version);
    }

    public Packets.SubtitleUserSettings ToPacket()
    {
        Packets.SubtitleUserSettings packet = new()
        {
            Version = (int)Version,
            FontSize = FontSize,
            BackgroundOpacity = BackgroundOpacity,
            SpeakerQualifierDisplay = SpeakerQualifierDisplay,
            BPostPlayerSubtitles = PostPlayerSubtitles,
            BPostPlayerSubtitlesToChat = PostPlayerSubtitlesToChat,
            NamesToShowMask = NamesToShowMask
        };
        return packet;
    }
}