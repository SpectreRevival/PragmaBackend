CREATE TABLE IF NOT EXISTS profile_data (
	player_id UUID PRIMARY KEY,
	display_name DisplayName NOT NULL,
	banner_item_id TEXT NOT NULL,
	crew_score BIGINT NOT NULL,
	current_solo_rank INT NOT NULL,
	highest_team_rank INT NOT NULL,
	division_type TEXT NOT NULL,

	CONSTRAINT verify_display_name CHECK (
		(display_name).player_name IS NOT NULL AND
		(display_name).discriminator IS NOT NULL
	)
);