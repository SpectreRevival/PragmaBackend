CREATE UNIQUE INDEX IF NOT EXISTS idx_profile_data_provider_account
ON profile_data (account_id_provider, provider_account_id);