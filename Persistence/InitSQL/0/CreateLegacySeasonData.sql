CREATE TABLE IF NOT EXISTS LegacySeasonData (
	player_id UUID PRIMARY KEY,
	solo_ranked_points BIGINT NOT NULL DEFAULT 0,
	current_solo_rank BIGINT NOT NULL DEFAULT 0
);