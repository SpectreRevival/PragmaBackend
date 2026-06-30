CREATE TABLE IF NOT EXISTS client_messages (
	message_id UUID PRIMARY KEY,
	player_id UUID NOT NULL,
	message_type TEXT NOT NULL,
	message_sender TEXT NOT NULL,
	message_sender_type TEXT NOT NULL,
	campaign_id TEXT NOT NULL,
	message_title TEXT NOT NULL,
	message_body TEXT NOT NULL,
	item_attachment_catalog_id TEXT,
	sent_time TIMESTAMPTZ NOT NULL,
	read_time TIMESTAMPTZ NOT NULL,
	expiration_time TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_client_messages_campaign_id ON client_messages(campaign_id);