DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('PartyMember')) THEN
		CREATE TYPE PartyMember AS (
			player_id UUID,
			is_ready BOOL,
			is_leader BOOL,
			preferred_team TEXT,
			ranked_mode_unlocked BOOL,
			party_member_version BIGINT,
			region TEXT
		);
	END IF;
END
$$;

DO $$
BEGIN
	IF NOT EXISTS (
		SELECT 1
		FROM pg_attribute a
		JOIN pg_type t ON t.typrelid = a.attrelid
		WHERE t.typname = 'partymember'
		AND a.attname = 'region'
		AND NOT a.attisdropped
	) THEN
		ALTER TYPE PartyMember ADD ATTRIBUTE region TEXT CASCADE;
	END IF;
END
$$;