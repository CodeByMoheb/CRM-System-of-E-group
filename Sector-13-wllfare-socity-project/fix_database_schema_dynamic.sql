-- Schema-aware database fix script for the Book Us system tables
-- Update the USE statement below if you need to target a specific database explicitly.
-- Example: USE [egroupbd];

DECLARE @targetDatabase sysname = DB_NAME();
PRINT 'Running schema fix script against database: ' + @targetDatabase;

DECLARE @tableName sysname;
DECLARE @schemaName sysname;
DECLARE @qualifiedName nvarchar(400);
DECLARE @sql nvarchar(max);

DECLARE @tablesNeedingUpdatedAt TABLE (TableName sysname);
INSERT INTO @tablesNeedingUpdatedAt (TableName) VALUES
    ('Services'),
    ('LocationCharges'),
    ('ManPowers'),
    ('CompanyCals'),
    ('Customers'),
    ('Invoices'),
    ('Orders'),
    ('OrderItems'),
    ('PaymentRecords'),
    ('CartItems');

DECLARE updatedAtCursor CURSOR FAST_FORWARD FOR
    SELECT TableName FROM @tablesNeedingUpdatedAt;

OPEN updatedAtCursor;
FETCH NEXT FROM updatedAtCursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @schemaName = NULL;

    SELECT TOP(1) @schemaName = s.name
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = @tableName;

    IF @schemaName IS NOT NULL
    BEGIN
        SET @qualifiedName = QUOTENAME(@schemaName) + '.[' + @tableName + ']';
        SET @sql = N'IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @qualifiedName + N''') AND name = ''UpdatedAt'')
BEGIN
    ALTER TABLE ' + @qualifiedName + N' ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END';
        EXEC sp_executesql @sql;
    END
    ELSE
    BEGIN
        PRINT 'Skipped UpdatedAt check because table ' + @tableName + ' was not found.';
    END

    FETCH NEXT FROM updatedAtCursor INTO @tableName;
END

CLOSE updatedAtCursor;
DEALLOCATE updatedAtCursor;

-- Mark migrations as applied so EF Core stops attempting to recreate existing schema objects
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250912200051_AddBookUsSystem')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250912200051_AddBookUsSystem', '8.0.8');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250913100220_AddBookingUpdates')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250913100220_AddBookingUpdates', '8.0.8');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250922125637_FixCompanyCalForeignKeyTypes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250922125637_FixCompanyCalForeignKeyTypes', '8.0.8');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250922143449_AddUpdatedAtColumn')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250922143449_AddUpdatedAtColumn', '8.0.8');
END

-- Ensure BookingItems.ServiceId allows null and uses ON DELETE SET NULL regardless of schema owner
DECLARE @bookingItemsSchema sysname;
DECLARE @servicesSchema sysname;
DECLARE @qualifiedBookingItems nvarchar(400);
DECLARE @qualifiedServices nvarchar(400);

SELECT TOP(1) @bookingItemsSchema = s.name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name = 'BookingItems';

SELECT TOP(1) @servicesSchema = s.name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name = 'Services';

IF @bookingItemsSchema IS NOT NULL
BEGIN
    SET @qualifiedBookingItems = QUOTENAME(@bookingItemsSchema) + '.[BookingItems]';

    SET @sql = N'IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = ''FK_BookingItems_Services_ServiceId'')
BEGIN
    ALTER TABLE ' + @qualifiedBookingItems + N' DROP CONSTRAINT [FK_BookingItems_Services_ServiceId];
END';
    EXEC sp_executesql @sql;

    SET @sql = N'IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @qualifiedBookingItems + N''') AND name = ''ServiceId'')
BEGIN
    ALTER TABLE ' + @qualifiedBookingItems + N' ALTER COLUMN [ServiceId] INT NULL;
END';
    EXEC sp_executesql @sql;

    IF @servicesSchema IS NOT NULL
    BEGIN
        SET @qualifiedServices = QUOTENAME(@servicesSchema) + '.[Services]';
        SET @sql = N'IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = ''FK_BookingItems_Services_ServiceId'')
BEGIN
    ALTER TABLE ' + @qualifiedBookingItems + N' ADD CONSTRAINT [FK_BookingItems_Services_ServiceId]
        FOREIGN KEY ([ServiceId]) REFERENCES ' + @qualifiedServices + N'([Id]) ON DELETE SET NULL;
END';
        EXEC sp_executesql @sql;
    END
    ELSE
    BEGIN
        PRINT 'Services table not found; skipped recreating FK_BookingItems_Services_ServiceId.';
    END
END
ELSE
BEGIN
    PRINT 'BookingItems table not found; skipped ServiceId adjustments.';
END

-- Ensure CompanyCals.Status exists for whichever schema owns the table
DECLARE @companyCalsSchema sysname;
DECLARE @qualifiedCompanyCals nvarchar(400);

SELECT TOP(1) @companyCalsSchema = s.name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name = 'CompanyCals';

IF @companyCalsSchema IS NOT NULL
BEGIN
    SET @qualifiedCompanyCals = QUOTENAME(@companyCalsSchema) + '.[CompanyCals]';
    SET @sql = N'IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @qualifiedCompanyCals + N''') AND name = ''Status'')
BEGIN
    ALTER TABLE ' + @qualifiedCompanyCals + N' ADD [Status] nvarchar(max) NULL;
END';
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'CompanyCals table not found; skipped Status column check.';
END

PRINT 'Database schema updated successfully!';