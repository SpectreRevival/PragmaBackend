DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('OutfitData')) THEN
		CREATE TYPE OutfitData AS (
			item_instance_id UUID,
			alteration_data ActiveAlterationData[],
			item_catalog_id UUID
		);
	END IF;
END
$$;

CREATE TABLE IF NOT EXISTS outfit_loadouts (
    player_id UUID PRIMARY KEY NOT NULL,
    loadout_id UUID NOT NULL DEFAULT gen_random_uuid(),
    head OUTFITDATA NOT NULL,
    hair OUTFITDATA NOT NULL,
    face_style OUTFITDATA NOT NULL,
    face_accessory OUTFITDATA NOT NULL,
    outfit OUTFITDATA NOT NULL,

    CONSTRAINT verify_head CHECK (
        (head).item_instance_id IS NOT NULL
        AND (head).alteration_data IS NOT NULL
        AND (head).item_catalog_id IS NOT NULL
    ),
    CONSTRAINT verify_hair CHECK (
        (hair).item_instance_id IS NOT NULL
        AND (hair).alteration_data IS NOT NULL
        AND (hair).item_catalog_id IS NOT NULL
    ),
    CONSTRAINT verify_face_style CHECK (
        (face_style).item_instance_id IS NOT NULL
        AND (face_style).alteration_data IS NOT NULL
        AND (face_style).item_catalog_id IS NOT NULL
    ),
    CONSTRAINT verify_face_accessory CHECK (
        (face_accessory).item_instance_id IS NOT NULL
        AND (face_accessory).alteration_data IS NOT NULL
        AND (face_accessory).item_catalog_id IS NOT NULL
    ),
    CONSTRAINT verify_outfit CHECK (
        (outfit).item_instance_id IS NOT NULL
        AND (outfit).alteration_data IS NOT NULL
        AND (outfit).item_catalog_id IS NOT NULL
    )
);
