CREATE TABLE IF NOT EXISTS friends_list (
    player_id UUID PRIMARY KEY,
    accepting_friend_invites BOOLEAN NOT NULL DEFAULT TRUE,
    friends UUID [] NOT NULL DEFAULT ARRAY[]::UUID [],
    blocked UUID [] NOT NULL DEFAULT ARRAY[]::UUID [],
    sent_friend_invites UUID [] NOT NULL DEFAULT ARRAY[]::UUID [],
    received_friend_invites UUID [] NOT NULL DEFAULT ARRAY[]::UUID [],
    list_version BIGINT NOT NULL DEFAULT 1
);
