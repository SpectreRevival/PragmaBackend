DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'PartyMember') THEN
		CREATE TYPE PartyMember AS (
			player_id UUID,
			is_ready BOOL,
			is_leader BOOL,
			preferred_team TEXT,
			ranked_mode_unlocked BOOL
		);
	END IF;
END
$$;