using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAllMissingColumnsAndForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure Services table has all required columns
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
                BEGIN
                    -- Add Registration_fees if not exists
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Registration_fees')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [Registration_fees] decimal(18,2) NULL;
                        PRINT 'Registration_fees column added to Services table.';
                    END
                    
                    -- Add CompanyCalId if not exists
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'CompanyCalId')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [CompanyCalId] nvarchar(50) NULL;
                        PRINT 'CompanyCalId column added to Services table.';
                    END
                    
                    -- Add DisplayOrder if not exists
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'DisplayOrder')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [DisplayOrder] int NOT NULL DEFAULT(0);
                        PRINT 'DisplayOrder column added to Services table.';
                    END
                    
                    -- Add IsActive if not exists
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'IsActive')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [IsActive] bit NOT NULL DEFAULT(1);
                        PRINT 'IsActive column added to Services table.';
                    END
                    
                    -- Add UpdatedAt if not exists
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'UpdatedAt')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [UpdatedAt] datetime2 NULL;
                        PRINT 'UpdatedAt column added to Services table.';
                    END
                END
            ");

            // Ensure Customers table has CompanyCalId
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'CompanyCalId')
                    BEGIN
                        ALTER TABLE [dbo].[Customers] ADD [CompanyCalId] nvarchar(50) NULL;
                        PRINT 'CompanyCalId column added to Customers table.';
                    END
                END
            ");

            // Ensure Invoices table has CompanyCalId
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'CompanyCalId')
                    BEGIN
                        ALTER TABLE [dbo].[Invoices] ADD [CompanyCalId] nvarchar(50) NULL;
                        PRINT 'CompanyCalId column added to Invoices table.';
                    END
                END
            ");

            // Create CompanyCals table if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[CompanyCals] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [CustomerId] nvarchar(50) NULL,
                        [VAT] decimal(18,2) NULL,
                        [LocationChargeId] int NULL,
                        [ManPowerId] int NULL,
                        [Total] decimal(18,2) NULL,
                        [Discount] decimal(18,2) NULL,
                        [InvoiceId] nvarchar(50) NULL,
                        [PaymentStatus] nvarchar(20) NULL DEFAULT 'UnPaid',
                        [ServiceId] int NULL,
                        [IsApproved] bit NOT NULL DEFAULT 0,
                        [IsDelete] bit NOT NULL DEFAULT 0,
                        [CreatedBy] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        [ApprovedBy] nvarchar(max) NULL,
                        [ApprovedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_CompanyCals] PRIMARY KEY ([Id])
                    );
                    
                    -- Create indexes for foreign keys
                    CREATE INDEX [IX_CompanyCals_LocationChargeId] ON [CompanyCals]([LocationChargeId]);
                    CREATE INDEX [IX_CompanyCals_ManPowerId] ON [CompanyCals]([ManPowerId]);
                    CREATE INDEX [IX_CompanyCals_ServiceId] ON [CompanyCals]([ServiceId]);
                    
                    PRINT 'CompanyCals table created successfully.';
                END
                ELSE
                BEGIN
                    PRINT 'CompanyCals table already exists.';
                END
            ");

            // Add foreign key constraints if they don't exist
            migrationBuilder.Sql(@"
                -- Add foreign key for LocationChargeId if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_LocationCharges_LocationChargeId')
                    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LocationCharges]') AND type in (N'U'))
                    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [CompanyCals] ADD CONSTRAINT [FK_CompanyCals_LocationCharges_LocationChargeId] 
                    FOREIGN KEY([LocationChargeId]) REFERENCES [LocationCharges]([Id]) ON DELETE SET NULL;
                    PRINT 'Foreign key constraint added for LocationChargeId.';
                END
                
                -- Add foreign key for ManPowerId if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_ManPowers_ManPowerId')
                    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManPowers]') AND type in (N'U'))
                    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [CompanyCals] ADD CONSTRAINT [FK_CompanyCals_ManPowers_ManPowerId] 
                    FOREIGN KEY([ManPowerId]) REFERENCES [ManPowers]([Id]) ON DELETE SET NULL;
                    PRINT 'Foreign key constraint added for ManPowerId.';
                END
                
                -- Add foreign key for ServiceId if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Services_ServiceId')
                    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
                    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [CompanyCals] ADD CONSTRAINT [FK_CompanyCals_Services_ServiceId] 
                    FOREIGN KEY([ServiceId]) REFERENCES [Services]([Id]) ON DELETE SET NULL;
                    PRINT 'Foreign key constraint added for ServiceId.';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key constraints
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_LocationCharges_LocationChargeId')
                BEGIN
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_LocationCharges_LocationChargeId];
                END
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_ManPowers_ManPowerId')
                BEGIN
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_ManPowers_ManPowerId];
                END
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Services_ServiceId')
                BEGIN
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Services_ServiceId];
                END
            ");

            // Remove added columns
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Registration_fees')
                BEGIN
                    ALTER TABLE [dbo].[Services] DROP COLUMN [Registration_fees];
                END
                
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Services] DROP COLUMN [CompanyCalId];
                END
                
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Customers] DROP COLUMN [CompanyCalId];
                END
                
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Invoices] DROP COLUMN [CompanyCalId];
                END
            ");
        }
    }
}