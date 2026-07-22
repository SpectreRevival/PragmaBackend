COPY battlepass_data 
(
player_id,
active_battle_passes,
battlepass_quests,
active_battlepass_quests,
battlepass_level
) FROM STDIN (FORMAT BINARY);