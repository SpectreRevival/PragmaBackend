COPY individual_tracked_progression (
player_id,
active_daily_quests,
active_weekly_quests,
active_event_quests,
active_endorsement,
last_rollover
) FROM STDIN (FORMAT BINARY);