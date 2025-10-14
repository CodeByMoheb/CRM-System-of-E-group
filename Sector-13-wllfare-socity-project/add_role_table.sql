-- Create Role table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
    
    -- Insert some default roles
    INSERT INTO [dbo].[Roles] ([Name], [Description], [IsActive], [CreatedAt])
    VALUES 
        ('Office Manager', 'Manages office operations and staff', 1, GETUTCDATE()),
        ('Security Guard', 'Provides security and safety services', 1, GETUTCDATE()),
        ('Cleaner', 'Maintains cleanliness and hygiene', 1, GETUTCDATE()),
        ('Driver', 'Provides transportation services', 1, GETUTCDATE()),
        ('Receptionist', 'Handles front desk and customer service', 1, GETUTCDATE());
END

-- Update Employee table to use RoleId instead of Role string
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employees' AND COLUMN_NAME = 'Role')
BEGIN
    -- Add RoleId column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employees' AND COLUMN_NAME = 'RoleId')
    BEGIN
        ALTER TABLE [dbo].[Employees] ADD [RoleId] int NULL;
    END
    
    -- Add foreign key constraint
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Employees_Roles_RoleId')
    BEGIN
        ALTER TABLE [dbo].[Employees] 
        ADD CONSTRAINT [FK_Employees_Roles_RoleId] 
        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]);
    END
    
    -- Update existing employees to use RoleId (assign to first role as default)
    UPDATE [dbo].[Employees] 
    SET [RoleId] = (SELECT TOP 1 [Id] FROM [dbo].[Roles] WHERE [IsActive] = 1)
    WHERE [RoleId] IS NULL;
    
    -- Make RoleId NOT NULL
    ALTER TABLE [dbo].[Employees] ALTER COLUMN [RoleId] int NOT NULL;
    
    -- Drop the old Role column
    ALTER TABLE [dbo].[Employees] DROP COLUMN [Role];
END

-- Remove ShiftId column if it exists (we're simplifying the system)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employees' AND COLUMN_NAME = 'ShiftId')
BEGIN
    ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [FK_Employees_Shifts_ShiftId];
    ALTER TABLE [dbo].[Employees] DROP COLUMN [ShiftId];
END

-- Remove Category column if it exists (we're simplifying the system)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employees' AND COLUMN_NAME = 'Category')
BEGIN
    ALTER TABLE [dbo].[Employees] DROP COLUMN [Category];
END

PRINT 'Role management system setup completed successfully!';
