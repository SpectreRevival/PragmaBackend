CREATE TABLE IF NOT EXISTS individual_tracked_progression (
    player_id UUID PRIMARY KEY,
    active_daily_quests TEXT [] NOT NULL,
    active_weekly_quests TEXT [] NOT NULL,
    active_event_quests TEXT [] NOT NULL,
    active_endorsement TEXT NOT NULL,
    last_rollover TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS team_tracked_progression (
    player_id UUID PRIMARY KEY,
    team_id TEXT NOT NULL,
    active_daily_quests TEXT [] NOT NULL,
    active_weekly_quests TEXT [] NOT NULL,
    active_event_quests TEXT [] NOT NULL,
    active_endorsement TEXT NOT NULL,
    last_rollover TIMESTAMPTZ NOT NULL
);
