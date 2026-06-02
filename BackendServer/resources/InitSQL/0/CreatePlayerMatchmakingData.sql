CREATE TABLE IF NOT EXISTS player_matchmaking_data (
    player_id UUID PRIMARY KEY,
    casual_mmr DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    ranked_mmr DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    solo_rank_points BIGINT NOT NULL DEFAULT 0,
    casual_matches_played BIGINT NOT NULL DEFAULT 0,
    ranked_matches_played BIGINT NOT NULL DEFAULT 0,
    casual_matches_played_seasonal BIGINT NOT NULL DEFAULT 0,
    ranked_matches_played_seasonal BIGINT NOT NULL DEFAULT 0,
    ranked_placement_matches TEXT [] NOT NULL DEFAULT ARRAY[]::TEXT [],
    current_solo_rank INT NOT NULL DEFAULT 0,
    highest_team_rank INT NOT NULL DEFAULT 0,
    casual_matches_won BIGINT NOT NULL DEFAULT 0,
    ranked_matches_won BIGINT NOT NULL DEFAULT 0,
    priority_matchmaking_until TIMESTAMPTZ NOT NULL DEFAULT to_timestamp(0),
    restrict_matchmaking_until TIMESTAMPTZ NOT NULL DEFAULT to_timestamp(0),
    map_history TEXT NOT NULL DEFAULT ''
);
