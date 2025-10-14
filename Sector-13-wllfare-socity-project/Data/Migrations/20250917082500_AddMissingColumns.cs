using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Registration_fees column to Services table if it doesn't exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Registration_fees')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [Registration_fees] decimal(18,2) NULL;
                        PRINT 'Registration_fees column added to Services table.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'Registration_fees column already exists in Services table.';
                    END
                END
            ");

            // Add CompanyCalId column to Services table if it doesn't exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'CompanyCalId')
                    BEGIN
                        ALTER TABLE [dbo].[Services] ADD [CompanyCalId] nvarchar(50) NULL;
                        PRINT 'CompanyCalId column added to Services table.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'CompanyCalId column already exists in Services table.';
                    END
                END
            ");

            // Add CompanyCalId column to Customers table if it doesn't exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'CompanyCalId')
                    BEGIN
                        ALTER TABLE [dbo].[Customers] ADD [CompanyCalId] nvarchar(50) NULL;
                        PRINT 'CompanyCalId column added to Customers table.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'CompanyCalId column already exists in Customers table.';
                    END
                END
            ");

            // Add CompanyCalId column to Invoices table if it doesn't exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'CompanyCalId')
                    BEGIN
                        ALTER TABLE [dbo].[Invoices] ADD [CompanyCalId] nvarchar(50) NULL;
                        PRINT 'CompanyCalId column added to Invoices table.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'CompanyCalId column already exists in Invoices table.';
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the added columns if they exist
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
        }
    }
}