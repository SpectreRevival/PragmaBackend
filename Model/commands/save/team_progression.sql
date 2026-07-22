INSERT INTO team_tracked_progression (
	player_id,
	team_id,
	active_daily_quests,
	active_weekly_quests,
	active_event_quests,
	last_rollover
) VALUES (
	@player_id,
	@team_id,
	@active_daily_quests,
	@active_weekly_quests,
	@active_event_quests,
	@last_rollover
) ON CONFLICT (player_id)
DO UPDATE SET
	team_id = EXCLUDED.team_id,
	active_daily_quests = EXCLUDED.active_daily_quests,
	active_weekly_quests = EXCLUDED.active_weekly_quests,
	active_event_quests = EXCLUDED.active_event_quests,
	last_rollover = EXCLUDED.last_rollover;