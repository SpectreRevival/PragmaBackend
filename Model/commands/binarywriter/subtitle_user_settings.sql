COPY subtitle_settings (
player_id,
font_size,
background_opacity,
speaker_qualifier_display,
post_player_subtitles,
post_player_subtitles_to_chat,
names_to_show_mask,
subtitle_settings_version
) FROM STDIN (FORMAT BINARY);