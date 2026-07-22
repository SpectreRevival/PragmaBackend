INSERT INTO outfit_loadouts (
	loadout_id,
	player_id,
	head,
	hair,
	face_style,
	face_accessory,
	outfit
) VALUES (
	@loadout_id,
	@player_id,
	@head,
	@hair,
	@face_style,
	@face_accessory,
	@outfit
) ON CONFLICT (loadout_id)
DO UPDATE SET
	player_id = EXCLUDED.player_id,
	head = EXCLUDED.head,
	hair = EXCLUDED.hair,
	face_style = EXCLUDED.face_style,
	face_accessory = EXCLUDED.face_accessory,
	outfit = EXCLUDED.outfit;