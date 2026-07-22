COPY player_presence (
player_id,
basic_status,
advanced_presence_type,
advanced_presence_context,
last_updated_time
) FROM STDIN (FORMAT BINARY);