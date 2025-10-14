USE [e-groupltd];

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Services]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [Services] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[LocationCharges]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [LocationCharges] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ManPowers]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [ManPowers] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CompanyCals]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [CompanyCals] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Customers]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [Customers] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Invoices]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [Invoices] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Orders]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [Orders] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrderItems]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [OrderItems] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[PaymentRecords]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [PaymentRecords] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CartItems]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [CartItems] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE();
END

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

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingItems_Services_ServiceId')
BEGIN
    ALTER TABLE [BookingItems] DROP CONSTRAINT [FK_BookingItems_Services_ServiceId];
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[BookingItems]') AND name = 'ServiceId')
BEGIN
    ALTER TABLE [BookingItems]
    ALTER COLUMN [ServiceId] INT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingItems_Services_ServiceId')
BEGIN
    ALTER TABLE [BookingItems]
    ADD CONSTRAINT [FK_BookingItems_Services_ServiceId]
        FOREIGN KEY ([ServiceId]) REFERENCES [Services]([Id]) ON DELETE SET NULL;
END

PRINT 'Database schema updated successfully!';
