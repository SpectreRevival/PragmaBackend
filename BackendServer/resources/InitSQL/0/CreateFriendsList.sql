CREATE TABLE IF NOT EXISTS friends_list (
	player_id UUID PRIMARY KEY,
	accepting_friend_invites BOOLEAN NOT NULL DEFAULT TRUE,
	friends TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	blocked TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	sent_friend_invites TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	received_friend_invites TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
	list_version BIGINT NOT NULL DEFAULT 1
);