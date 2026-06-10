DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('PlayerBasicPresence')) THEN
        CREATE TYPE PlayerBasicPresence AS ENUM (
            'Online', 
            'Offline'
        );
    END IF;
END
$$;
