INSERT INTO friends_list (
	player_id,
	accepting_friend_invites,
	friends,
	blocked,
	sent_friend_invites,
	received_friend_invites,
	list_version
) VALUES (
	@player_id,
	@accepting_friend_invites,
	@friends,
	@blocked,
	@sent_friend_invites,
	@received_friend_invites,
	@version
) ON CONFLICT (player_id)
DO UPDATE SET
	accepting_friend_invites = EXCLUDED.accepting_friend_invites,
	friends = EXCLUDED.friends,
	blocked = EXCLUDED.blocked,
	sent_friend_invites = EXCLUDED.sent_friend_invites,
	received_friend_invites = EXCLUDED.received_friend_invites,
	list_version = EXCLUDED.list_version;