CREATE OR REPLACE FUNCTION all_party_members_nonnull(arr PARTYMEMBER [])
RETURNS BOOLEAN AS $$
	SELECT NOT EXISTS (
		SELECT 1
		FROM unnest(arr) as unpacked_member
		WHERE unpacked_member.player_id IS NULL
		OR unpacked_member.is_ready IS NULL
		OR unpacked_member.is_leader IS NULL
		OR unpacked_member.preferred_team IS NULL
		OR unpacked_member.ranked_mode_unlocked IS NULL
	);
$$ LANGUAGE sql IMMUTABLE;

CREATE OR REPLACE FUNCTION at_least_one_member_leader(arr PARTYMEMBER [])
RETURNS BOOLEAN AS $$
	SELECT EXISTS (
		SELECT 1
		FROM unnest(arr) AS unpacked_member
		WHERE unpacked_member.is_leader IS TRUE
	);
$$ LANGUAGE sql IMMUTABLE;

CREATE TABLE IF NOT EXISTS parties (
    party_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    members PARTYMEMBER [] NOT NULL,
    invite_code TEXT NOT NULL,
    queue_pool TEXT NOT NULL,
    lobby_mode TEXT NOT NULL,
    chat_id TEXT NOT NULL,
    use_team_mmr BOOL NOT NULL,
	party_version BIGINT NOT NULL,

    CONSTRAINT verify_party_members CHECK (
        all_party_members_nonnull(members)
    ),
    CONSTRAINT at_least_one_leader CHECK (
        at_least_one_member_leader(members)
    )
);
