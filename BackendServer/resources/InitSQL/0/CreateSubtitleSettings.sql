CREATE TABLE IF NOT EXISTS subtitle_settings (
    player_id UUID PRIMARY KEY,
    font_size INT NOT NULL,
    background_opacity DOUBLE PRECISION NOT NULL,
    speaker_qualifier_display TEXT NOT NULL,
    post_player_subtitles BOOL NOT NULL,
    post_player_subtitles_to_chat BOOL NOT NULL,
    names_to_show_mask INT NOT NULL,
    subtitle_settings_version INT NOT NULL
);
