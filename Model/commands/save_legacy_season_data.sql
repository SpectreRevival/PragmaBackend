INSERT INTO legacy_season_data (
	player_id,
	solo_ranked_points,
	current_solo_rank
) VALUES (
	@player_id,
	@solo_ranked_points,
	@current_solo_rank
) ON CONFLICT (player_id)
DO UPDATE SET
	solo_ranked_points = EXCLUDED.solo_ranked_points,
	current_solo_rank = EXCLUDED.current_solo_rank;