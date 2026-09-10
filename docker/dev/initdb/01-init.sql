-- The backend connects to [origam-dev] as sa, so this just needs to exist.
-- Origam's deployment service creates the rest of the schema (including the
-- OrigamModelVersion/AsapModelVersion tables) on first boot.

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'origam-dev')
BEGIN
    CREATE DATABASE [origam-dev];
    PRINT 'Created origam-dev database';
END
ELSE
BEGIN
    PRINT 'origam-dev database already exists';
END
GO
