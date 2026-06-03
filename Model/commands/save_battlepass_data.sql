INSERT INTO battlepass_data (player_id, active_battle_passes, battlepass_quests, active_battlepass_quests, battlepass_level)
VALUES (@player_id, @active_battle_passes, @battlepass_quests, @active_battlepass_quests, @battlepass_level)
ON CONFLICT (player_id)
DO UPDATE SET
	active_battle_passes = EXCLUDED.active_battle_passes,
	battlepass_quests = EXCLUDED.battlepass_quests,
	active_battlepass_quests = EXCLUDED.active_battlepass_quests,
	battlepass_level = EXCLUDED.battlepass_level;