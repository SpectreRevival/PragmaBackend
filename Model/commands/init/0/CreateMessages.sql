DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('ClientMessageSender')) THEN
        CREATE TYPE ClientMessageSender AS (
            sender_type TEXT,
            sender TEXT
        );
    END IF;
END
$$;

CREATE OR REPLACE FUNCTION check_client_message_senders(senders ClientMessageSender[])
RETURNS BOOLEAN
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT NOT EXISTS (
        SELECT 1 
        FROM unnest(senders) AS s 
        WHERE s.sender_type IS NULL OR s.sender IS NULL
    );
$$;

CREATE TABLE IF NOT EXISTS client_messages (
    message_id UUID PRIMARY KEY,
    player_id UUID NOT NULL,
    message_type TEXT NOT NULL,
    senders ClientMessageSender[] NOT NULL,
    campaign_id TEXT NOT NULL,
    message_title TEXT NOT NULL,
    message_body TEXT NOT NULL,
    item_attachment_catalog_id TEXT,
    sent_time TIMESTAMPTZ NOT NULL,
    read_time TIMESTAMPTZ NOT NULL,
    expiration_time TIMESTAMPTZ NOT NULL,
    CONSTRAINT ensure_senders_not_null CHECK (check_client_message_senders(senders))
);

CREATE INDEX IF NOT EXISTS idx_client_messages_campaign_id ON client_messages(campaign_id);