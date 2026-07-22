COPY team_tracked_progression (
player_id,
team_id,
active_daily_quests,
active_weekly_quests,
active_event_quests,
last_rollover
) FROM STDIN (FORMAT BINARY);