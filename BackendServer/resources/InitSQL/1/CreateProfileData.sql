CREATE TABLE IF NOT EXISTS profile_data (
	player_id UUID PRIMARY KEY,
	display_name DisplayName NOT NULL,
	banner_item_id TEXT NOT NULL,
	pre_spray_item_id TEXT NOT NULL,
	match_spray_item_id TEXT NOT NULL,
	post_spray_item_id TEXT NOT NULL,
	attacker_outfit_loadout_id TEXT NOT NULL,
	defender_outfit_loadout_id TEXT NOT NULL,
	attacker_weapon_loadout_id TEXT NOT NULL,
	defender_weapon_loadout_id TEXT NOT NULL,
	last_updated TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
	next_new_day_rollover TIMESTAMPTZ NOT NULL,
	last_login TIMESTAMPTZ NOT NULL,
	player_flags INT NOT NULL,
	crew_score BIGINT NOT NULL,
	current_solo_rank INT NOT NULL,
	highest_team_rank INT NOT NULL,
	division_type TEXT NOT NULL,
	inventory_version INT NOT NULL DEFAULT 0,
	crew_id TEXT NOT NULL,
	account_id_provider TEXT NOT NULL,
	platform_name TEXT NOT NULL,
	provider_account_id TEXT NOT NULL,
	crossplay_platform_kind TEXT NOT NULL,
	games_remaining_until_crew_join INT NOT NULL,

	CONSTRAINT verify_display_name CHECK (
		(display_name).player_name IS NOT NULL AND
		(display_name).discriminator IS NOT NULL
	)
);

CREATE OR REPLACE FUNCTION profile_data_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
	NEW.last_updated = NOW();
	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trigger_profile_set_tz ON profile_data;
CREATE TRIGGER trigger_profile_set_tz BEFORE UPDATE ON profile_data FOR EACH ROW EXECUTE FUNCTION profile_data_set_timestamp();