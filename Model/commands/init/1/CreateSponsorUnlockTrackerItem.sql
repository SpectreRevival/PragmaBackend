CREATE TABLE IF NOT EXISTS sponsor_tracker_items (
	instance_id UUID PRIMARY KEY,
	catalog_id TEXT NOT NULL,
	owning_player_id UUID NOT NULL,
	viewed BOOL NOT NULL,
	sponsor_name TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_sponsor_tracker_items_by_player ON sponsor_tracker_items (
	owning_player_id, instance_id
);