using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class SubtitleUserSettings : VersionedData, IDatabaseSyncable<SubtitleUserSettings, Guid>, IEquatable<SubtitleUserSettings>
{
    [SetsRequiredMembers]
    public SubtitleUserSettings(Guid playerId, int fontSize, double backgroundOpacity, string speakerQualifierDisplay, bool postPlayerSubtitles, bool postPlayerSubtitlesToChat, int namesToShowMask, Int64 version)
    {
        PlayerId = playerId;
        FontSize = fontSize;
        BackgroundOpacity = backgroundOpacity;
        SpeakerQualifierDisplay = speakerQualifierDisplay ?? throw new ArgumentNullException(nameof(speakerQualifierDisplay));
        PostPlayerSubtitles = postPlayerSubtitles;
        PostPlayerSubtitlesToChat = postPlayerSubtitlesToChat;
        NamesToShowMask = namesToShowMask;
        Version = version;
    }

    public required Guid PlayerId { get; set; }
    public required Int32 FontSize { get; set; }
    public required double BackgroundOpacity { get; set; }
    public required string SpeakerQualifierDisplay { get; set; }
    public required bool PostPlayerSubtitles { get; set; }
    public required bool PostPlayerSubtitlesToChat { get; set; }
    public required Int32 NamesToShowMask { get; set; }

    public static async Task<SubtitleUserSettings?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_subtitle_settings.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new SubtitleUserSettings(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Int32>(1),
            await reader.GetFieldValueAsync<double>(2),
            await reader.GetFieldValueAsync<string>(3),
            await reader.GetFieldValueAsync<bool>(4),
            await reader.GetFieldValueAsync<bool>(5),
            await reader.GetFieldValueAsync<Int32>(6),
            await reader.GetFieldValueAsync<Int64>(7)
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
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && FontSize == other.FontSize
            && BackgroundOpacity == other.BackgroundOpacity
            && SpeakerQualifierDisplay == other.SpeakerQualifierDisplay
            && PostPlayerSubtitles == other.PostPlayerSubtitles
            && PostPlayerSubtitlesToChat == other.PostPlayerSubtitlesToChat
            && NamesToShowMask == other.NamesToShowMask
            && Version == other.Version;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
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
}