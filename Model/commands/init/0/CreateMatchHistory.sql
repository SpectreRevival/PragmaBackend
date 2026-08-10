CREATE TABLE IF NOT EXISTS match_history (
	match_id UUID PRIMARY KEY,
	match_date TIMESTAMPTZ NOT NULL,
	queue_name TEXT NOT NULL,
	queue_game_mode TEXT NOT NULL,
	queue_game_map TEXT NOT NULL,
	overtime_enabled BOOL NOT NULL,
	region TEXT NOT NULL,
	is_ranked BOOL NOT NULL,
	is_abandoned_match BOOL NOT NULL,
	abandoned_player_ids UUID[] NOT NULL,
	surrendered_team INT NOT NULL
);