using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingColumnsOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to Services table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Registration_fees')
                BEGIN
                    ALTER TABLE [dbo].[Services] ADD [Registration_fees] decimal(18,2) NOT NULL DEFAULT(0);
                    PRINT 'Registration_fees column added to Services table.';
                END
                ELSE
                BEGIN
                    PRINT 'Registration_fees column already exists in Services table.';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Services] ADD [CompanyCalId] nvarchar(50) NULL;
                    PRINT 'CompanyCalId column added to Services table.';
                END
                ELSE
                BEGIN
                    PRINT 'CompanyCalId column already exists in Services table.';
                END
            ");

            // Add missing columns to Customers table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Customers] ADD [CompanyCalId] nvarchar(50) NULL;
                    PRINT 'CompanyCalId column added to Customers table.';
                END
                ELSE
                BEGIN
                    PRINT 'CompanyCalId column already exists in Customers table.';
                END
            ");

            // Add missing columns to Invoices table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Invoices] ADD [CompanyCalId] nvarchar(50) NULL;
                    PRINT 'CompanyCalId column added to Invoices table.';
                END
                ELSE
                BEGIN
                    PRINT 'CompanyCalId column already exists in Invoices table.';
                END
            ");

            // Add missing CreatedAt columns to BuyerContacts and ClientContacts tables
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuyerContacts]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [dbo].[BuyerContacts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT(GETDATE());
                    PRINT 'CreatedAt column added to BuyerContacts table.';
                END
                ELSE
                BEGIN
                    PRINT 'CreatedAt column already exists in BuyerContacts table.';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ClientContacts]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [dbo].[ClientContacts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT(GETDATE());
                    PRINT 'CreatedAt column added to ClientContacts table.';
                END
                ELSE
                BEGIN
                    PRINT 'CreatedAt column already exists in ClientContacts table.';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the added columns
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Registration_fees')
                BEGIN
                    ALTER TABLE [dbo].[Services] DROP COLUMN [Registration_fees];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Services] DROP COLUMN [CompanyCalId];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Customers] DROP COLUMN [CompanyCalId];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'CompanyCalId')
                BEGIN
                    ALTER TABLE [dbo].[Invoices] DROP COLUMN [CompanyCalId];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuyerContacts]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [dbo].[BuyerContacts] DROP COLUMN [CreatedAt];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ClientContacts]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [dbo].[ClientContacts] DROP COLUMN [CreatedAt];
                END
            ");
        }
    }
}
