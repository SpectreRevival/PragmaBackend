CREATE TABLE IF NOT EXISTS battlepass_data (
	player_id UUID PRIMARY KEY,
	active_battle_passes TEXT[] NOT NULL,
	battlepass_quests TEXT[] NOT NULL,
	active_battlepass_quests TEXT[] NOT NULL,
	current_level INT NOT NULL
);