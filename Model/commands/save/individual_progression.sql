INSERT INTO individual_tracked_progression (
	player_id,
	active_daily_quests,
	active_weekly_quests,
	active_event_quests,
	active_endorsement,
	last_rollover
) VALUES (
	@player_id,
	@active_daily_quests,
	@active_weekly_quests,
	@active_event_quests,
	@active_endorsement,
	@last_rollover
) ON CONFLICT (player_id)
DO UPDATE SET
	active_daily_quests = EXCLUDED.active_daily_quests,
	active_weekly_quests = EXCLUDED.active_weekly_quests,
	active_event_quests = EXCLUDED.active_event_quests,
	active_endorsement = EXCLUDED.active_endorsement,
	last_rollover = EXCLUDED.last_rollover;