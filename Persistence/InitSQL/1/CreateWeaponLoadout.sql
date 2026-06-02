DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'WeaponAttachment') THEN
		CREATE TYPE WeaponAttachment AS (
			attachment_item_instance_id TEXT,
			attachment_item_catalog_id TEXT
		);
	END IF;
END
$$;

DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'WeaponData') THEN
		CREATE TYPE WeaponData AS (
			item_instance_id TEXT,
			alteration_data ActiveAlterationData[],
			item_catalog_id TEXT,
			attachment WeaponAttachment
		);
	END IF;
END
$$;

CREATE TABLE IF NOT EXISTS weapon_loadouts (
	player_id UUID PRIMARY KEY NOT NULL,
	loadout_id UUID NOT NULL DEFAULT gen_random_uuid(),
	semi_auto_pistol WeaponData NOT NULL,
	suppressed_pistol WeaponData NOT NULL,
	auto_pistol WeaponData NOT NULL,
	highcal_pistol WeaponData NOT NULL,
	heavy_shotgun WeaponData NOT NULL,
	auto_shotgun WeaponData NOT NULL,
	tactical_smg WeaponData NOT NULL,
	rapidfire_smg WeaponData NOT NULL,
	suppressed_smg WeaponData NOT NULL,
	standard_ar WeaponData NOT NULL,
	semi_auto_ar WeaponData NOT NULL,
	burst_ar WeaponData NOT NULL,
	tactical_ar WeaponData NOT NULL,
	suppressed_ar WeaponData NOT NULL,
	heavy_ar WeaponData NOT NULL,
	highcal_mg WeaponData NOT NULL,
	rapidfire_mg WeaponData NOT NULL,
	semi_auto_sniper WeaponData NOT NULL,
	bolt_action_sniper WeaponData NOT NULL,
	melee WeaponData NOT NULL,

	CONSTRAINT verify_semi_auto_pistol CHECK (
		(semi_auto_pistol).item_instance_id IS NOT NULL AND
		(semi_auto_pistol).alteration_data IS NOT NULL AND
		(semi_auto_pistol).item_catalog_id IS NOT NULL AND 
		(
        ((semi_auto_pistol).attachment) IS NULL 
        OR 
        (
            ((semi_auto_pistol).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((semi_auto_pistol).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_suppressed_pistol CHECK (
		(suppressed_pistol).item_instance_id IS NOT NULL AND
		(suppressed_pistol).alteration_data IS NOT NULL AND
		(suppressed_pistol).item_catalog_id IS NOT NULL AND 
		(
        ((suppressed_pistol).attachment) IS NULL 
        OR 
        (
            ((suppressed_pistol).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((suppressed_pistol).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_auto_pistol CHECK (
		(auto_pistol).item_instance_id IS NOT NULL AND
		(auto_pistol).alteration_data IS NOT NULL AND
		(auto_pistol).item_catalog_id IS NOT NULL AND 
		(
        ((auto_pistol).attachment) IS NULL 
        OR 
        (
            ((auto_pistol).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((auto_pistol).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_highcal_pistol CHECK (
		(highcal_pistol).item_instance_id IS NOT NULL AND
		(highcal_pistol).alteration_data IS NOT NULL AND
		(highcal_pistol).item_catalog_id IS NOT NULL AND 
		(
        ((highcal_pistol).attachment) IS NULL 
        OR 
        (
            ((highcal_pistol).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((highcal_pistol).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_heavy_shotgun CHECK (
		(heavy_shotgun).item_instance_id IS NOT NULL AND
		(heavy_shotgun).alteration_data IS NOT NULL AND
		(heavy_shotgun).item_catalog_id IS NOT NULL AND 
		(
        ((heavy_shotgun).attachment) IS NULL 
        OR 
        (
            ((heavy_shotgun).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((heavy_shotgun).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_auto_shotgun CHECK (
		(auto_shotgun).item_instance_id IS NOT NULL AND
		(auto_shotgun).alteration_data IS NOT NULL AND
		(auto_shotgun).item_catalog_id IS NOT NULL AND 
		(
        ((auto_shotgun).attachment) IS NULL 
        OR 
        (
            ((auto_shotgun).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((auto_shotgun).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_tactical_smg CHECK (
		(tactical_smg).item_instance_id IS NOT NULL AND
		(tactical_smg).alteration_data IS NOT NULL AND
		(tactical_smg).item_catalog_id IS NOT NULL AND 
		(
        ((tactical_smg).attachment) IS NULL 
        OR 
        (
            ((tactical_smg).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((tactical_smg).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_rapidfire_smg CHECK (
		(rapidfire_smg).item_instance_id IS NOT NULL AND
		(rapidfire_smg).alteration_data IS NOT NULL AND
		(rapidfire_smg).item_catalog_id IS NOT NULL AND 
		(
        ((rapidfire_smg).attachment) IS NULL 
        OR 
        (
            ((rapidfire_smg).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((rapidfire_smg).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_suppressed_smg CHECK (
		(suppressed_smg).item_instance_id IS NOT NULL AND
		(suppressed_smg).alteration_data IS NOT NULL AND
		(suppressed_smg).item_catalog_id IS NOT NULL AND 
		(
        ((suppressed_smg).attachment) IS NULL 
        OR 
        (
            ((suppressed_smg).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((suppressed_smg).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_standard_ar CHECK (
		(standard_ar).item_instance_id IS NOT NULL AND
		(standard_ar).alteration_data IS NOT NULL AND
		(standard_ar).item_catalog_id IS NOT NULL AND 
		(
        ((standard_ar).attachment) IS NULL 
        OR 
        (
            ((standard_ar).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((standard_ar).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_semi_auto_ar CHECK (
		(semi_auto_ar).item_instance_id IS NOT NULL AND
		(semi_auto_ar).alteration_data IS NOT NULL AND
		(semi_auto_ar).item_catalog_id IS NOT NULL AND 
		(
        ((semi_auto_ar).attachment) IS NULL 
        OR 
        (
            ((semi_auto_ar).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((semi_auto_ar).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_burst_ar CHECK (
		(burst_ar).item_instance_id IS NOT NULL AND
		(burst_ar).alteration_data IS NOT NULL AND
		(burst_ar).item_catalog_id IS NOT NULL AND 
		(
        ((burst_ar).attachment) IS NULL 
        OR 
        (
            ((burst_ar).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((burst_ar).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_tactical_ar CHECK (
		(tactical_ar).item_instance_id IS NOT NULL AND
		(tactical_ar).alteration_data IS NOT NULL AND
		(tactical_ar).item_catalog_id IS NOT NULL AND 
		(
        ((tactical_ar).attachment) IS NULL 
        OR 
        (
            ((tactical_ar).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((tactical_ar).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_suppressed_ar CHECK (
		(suppressed_ar).item_instance_id IS NOT NULL AND
		(suppressed_ar).alteration_data IS NOT NULL AND
		(suppressed_ar).item_catalog_id IS NOT NULL AND 
		(
        ((suppressed_ar).attachment) IS NULL 
        OR 
        (
            ((suppressed_ar).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((suppressed_ar).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_heavy_ar CHECK (
		(heavy_ar).item_instance_id IS NOT NULL AND
		(heavy_ar).alteration_data IS NOT NULL AND
		(heavy_ar).item_catalog_id IS NOT NULL AND 
		(
        ((heavy_ar).attachment) IS NULL 
        OR 
        (
            ((heavy_ar).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((heavy_ar).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_highcal_mg CHECK (
		(highcal_mg).item_instance_id IS NOT NULL AND
		(highcal_mg).alteration_data IS NOT NULL AND
		(highcal_mg).item_catalog_id IS NOT NULL AND 
		(
        ((highcal_mg).attachment) IS NULL 
        OR 
        (
            ((highcal_mg).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((highcal_mg).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_rapidfire_mg CHECK (
		(rapidfire_mg).item_instance_id IS NOT NULL AND
		(rapidfire_mg).alteration_data IS NOT NULL AND
		(rapidfire_mg).item_catalog_id IS NOT NULL AND 
		(
        ((rapidfire_mg).attachment) IS NULL 
        OR 
        (
            ((rapidfire_mg).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((rapidfire_mg).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_semi_auto_sniper CHECK (
		(semi_auto_sniper).item_instance_id IS NOT NULL AND
		(semi_auto_sniper).alteration_data IS NOT NULL AND
		(semi_auto_sniper).item_catalog_id IS NOT NULL AND 
		(
        ((semi_auto_sniper).attachment) IS NULL 
        OR 
        (
            ((semi_auto_sniper).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((semi_auto_sniper).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_bolt_action_sniper CHECK (
		(bolt_action_sniper).item_instance_id IS NOT NULL AND
		(bolt_action_sniper).alteration_data IS NOT NULL AND
		(bolt_action_sniper).item_catalog_id IS NOT NULL AND 
		(
        ((bolt_action_sniper).attachment) IS NULL 
        OR 
        (
            ((bolt_action_sniper).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((bolt_action_sniper).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	),
	CONSTRAINT verify_melee CHECK (
		(melee).item_instance_id IS NOT NULL AND
		(melee).alteration_data IS NOT NULL AND
		(melee).item_catalog_id IS NOT NULL AND 
		(
        ((melee).attachment) IS NULL 
        OR 
        (
            ((melee).attachment).attachment_item_instance_id IS NOT NULL AND 
            ((melee).attachment).attachment_item_catalog_id IS NOT NULL
        )
		)
	)
);