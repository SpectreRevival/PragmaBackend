INSERT INTO match_history (
match_id,
match_date,
queue_name,
queue_game_mode,
queue_game_map,
overtime_enabled,
region,
is_ranked,
is_abandoned_match,
abandoned_player_ids,
surrendered_team
)
VALUES (
	@match_id,
	@match_date,
	@queue_name,
	@queue_game_mode,
	@queue_game_map,
	@overtime_enabled,
	@region,
	@is_ranked,
	@is_abandoned_match,
	@abandoned_player_ids,
	@surrendered_team
) ON CONFLICT (match_id) DO UPDATE SET
	match_date = EXCLUDED.match_date,
	queue_name = EXCLUDED.queue_name,
	queue_game_mode = EXCLUDED.queue_game_mode,
	queue_game_map = EXCLUDED.queue_game_map,
	overtime_enabled = EXCLUDED.overtime_enabled,
	region = EXCLUDED.region,
	is_ranked = EXCLUDED.is_ranked,
	is_abandoned_match = EXCLUDED.is_abandoned_match,
	abandoned_player_ids = EXCLUDED.abandoned_player_ids,
	surrendered_team = EXCLUDED.surrendered_team;