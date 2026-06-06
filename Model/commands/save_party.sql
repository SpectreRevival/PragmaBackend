INSERT INTO parties (
	party_id,
	members,
	invite_code,
	queue_pool,
	lobby_mode,
	chat_id,
	use_team_mmr,
	party_version
) VALUES (
	@party_id,
	@members,
	@invite_code,
	@queue_pool,
	@lobby_mode,
	@chat_id,
	@use_team_mmr,
	@version
) ON CONFLICT (party_id)
DO UPDATE SET
	members = EXCLUDED.members,
	invite_code = EXCLUDED.invite_code,
	queue_pool = EXCLUDED.queue_pool,
	lobby_mode = EXCLUDED.lobby_mode,
	chat_id = EXCLUDED.chat_id,
	use_team_mmr = EXCLUDED.use_team_mmr,
	party_version = EXCLUDED.party_version;