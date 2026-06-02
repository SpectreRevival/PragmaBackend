DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typename = 'OutfitData') THEN
		CREATE TYPE OutfitData AS (
			item_instance_id TEXT,
			alteration_data ActiveAlterationData[],
			item_catalog_id TEXT
		);
	END IF;
END
$$;

CREATE TABLE IF NOT EXISTS outfit_loadouts (
	loadout_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	player_id UUID NOT NULL,
	head OutfitData NOT NULL,
	hair OutfitData NOT NULL,
	face_style OutfitData NOT NULL,
	face_accessory OutfitData NOT NULL,
	outfit OutfitData NOT NULL,

	CONSTRAINT verify_head CHECK (
		(head).item_instance_id IS NOT NULL AND
		(head).alterationData IS NOT NULL AND
		(head).item_catalog_id IS NOT NULL
	),
	CONSTRAINT verify_hair CHECK (
		(hair).item_instance_id IS NOT NULL AND
		(hair).alterationData IS NOT NULL AND
		(hair).item_catalog_id IS NOT NULL
	),
	CONSTRAINT verify_face_style CHECK (
		(face_style).item_instance_id IS NOT NULL AND
		(face_style).alterationData IS NOT NULL AND
		(face_style).item_catalog_id IS NOT NULL
	),
	CONSTRAINT verify_face_accessory CHECK (
		(face_accessory).item_instance_id IS NOT NULL AND
		(face_accessory).alterationData IS NOT NULL AND
		(face_accessory).item_catalog_id IS NOT NULL
	),
	CONSTRAINT verify_outfit CHECK (
		(outfit).item_instance_id IS NOT NULL AND
		(outfit).alterationData IS NOT NULL AND
		(outfit).item_catalog_id IS NOT NULL
	)
);