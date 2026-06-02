CREATE TABLE IF NOT EXISTS stackable_items (
    instance_id UUID PRIMARY KEY,
    catalog_id UUID NOT NULL,
    amount BIGINT NOT NULL,
    owning_player_id UUID NOT NULL
);

CREATE INDEX idx_stackable_items_by_player ON stackable_items (
    owning_player_id, instance_id
);
