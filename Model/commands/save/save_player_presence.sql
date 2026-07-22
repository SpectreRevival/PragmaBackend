INSERT INTO player_presence (
	player_id,
	basic_status,
	advanced_presence_type,
	advanced_presence_context,
	last_updated_time
) VALUES (
	@player_id,
	@basic_status,
	@advanced_presence_type,
	@advanced_presence_context,
	@last_updated_time
) ON CONFLICT (player_id)
DO UPDATE SET
	basic_status = EXCLUDED.basic_status,
	advanced_presence_type = EXCLUDED.advanced_presence_type,
	advanced_presence_context = EXCLUDED.advanced_presence_context,
	last_updated_time = EXCLUDED.last_updated_time;