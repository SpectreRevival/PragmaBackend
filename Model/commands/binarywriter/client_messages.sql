COPY client_messages (
message_id,
player_id,
message_type,
senders,
campaign_id,
message_title,
message_body,
item_attachment_catalog_id,
sent_time,
read_time,
expiration_time
) FROM STDIN (FORMAT BINARY);