INSERT INTO client_messages (
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
) VALUES (
	@message_id,
	@player_id,
	@message_type,
	@senders,
	@campaign_id,
	@message_title,
	@message_body,
	@item_attachment_catalog_id,
	@sent_time,
	@read_time,
	@expiration_time
) ON CONFLICT (message_id)
DO UPDATE SET
	player_id = EXCLUDED.player_id,
	message_type = EXCLUDED.message_type,
	senders = EXCLUDED.senders,
	campaign_id = EXCLUDED.campaign_id,
	message_title = EXCLUDED.message_title,
	message_body = EXCLUDED.message_body,
	item_attachment_catalog_id = EXCLUDED.item_attachment_catalog_id,
	sent_time = EXCLUDED.sent_time,
	read_time = EXCLUDED.read_time,
	expiration_time = EXCLUDED.expiration_time;