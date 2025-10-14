-- Create LocationCharges table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LocationCharges' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LocationCharges] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [LChargeType] nvarchar(max) NULL,
        [LChargeValue] decimal(18,2) NULL,
        [IsApproved] bit NOT NULL DEFAULT (1),
        [IsDelete] bit NOT NULL DEFAULT (0),
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_LocationCharges] PRIMARY KEY ([Id])
    );
    
    PRINT 'LocationCharges table created successfully.';
END
ELSE
BEGIN
    PRINT 'LocationCharges table already exists.';
END