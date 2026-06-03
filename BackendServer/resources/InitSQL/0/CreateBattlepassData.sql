CREATE TABLE IF NOT EXISTS battlepass_data (
    player_id UUID PRIMARY KEY,
    active_battle_passes UUID [] NOT NULL,
    battlepass_quests UUID [] NOT NULL,
    active_battlepass_quests UUID [] NOT NULL,
    battlepass_level INT NOT NULL
);
