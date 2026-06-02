CREATE TABLE IF NOT EXISTS friends_list (
	player_id UUID PRIMARY KEY,
	accepting_friend_invites BOOLEAN NOT NULL DEFAULT TRUE,
	friends TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	blocked TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	sent_friend_invites TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	received_friend_invites TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	list_version BIGINT NOT NULL DEFAULT 1
);

CREATE OR REPLACE FUNCTION increment_friends_list_row_version()
RETURNS TRIGGER AS $$
BEGIN
	NEW.row_version = OLD.row_version + 1;
	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_bump_version_friends_list ON friends_list;
CREATE TRIGGER trg_bump_version_friends_list BEFORE UPDATE ON friends_list FOR EACH ROW EXECUTE FUNCTION increment_friends_list_row_version();