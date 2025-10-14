-- Script to reset migration history and prepare for fresh migration application
USE [e-groupltd];

-- Clear the migration history table if it exists
IF OBJECT_ID('[__EFMigrationsHistory]', 'U') IS NOT NULL
BEGIN
    DELETE FROM [__EFMigrationsHistory];
    PRINT 'Migration history cleared';
END

-- Check if AspNetRoles table exists, if so we need a different approach
IF OBJECT_ID('[AspNetRoles]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetRoles table exists - database has some existing structure';
END
ELSE
BEGIN
    PRINT 'AspNetRoles table does not exist - database is empty';
END

PRINT 'Reset script completed';


