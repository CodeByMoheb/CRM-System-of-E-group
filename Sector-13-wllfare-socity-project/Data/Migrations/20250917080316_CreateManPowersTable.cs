﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateManPowersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ManPowers table if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManPowers]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[ManPowers] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [ManPowerType] nvarchar(max) NULL,
                        [ServiceId] int NULL,
                        [ManPowerDay] decimal(18,2) NULL,
                        [ManPowerPrice] decimal(18,2) NULL,
                        [IsApproved] bit NOT NULL DEFAULT 0,
                        [IsDelete] bit NOT NULL DEFAULT 0,
                        [CreatedBy] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        [ApprovedBy] nvarchar(max) NULL,
                        [ApprovedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ManPowers] PRIMARY KEY ([Id])
                    );
                    
                    -- Create index for ServiceId foreign key
                    CREATE INDEX [IX_ManPowers_ServiceId] ON [ManPowers]([ServiceId]);
                    
                    -- Add foreign key constraint to Services table if it exists
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
                    BEGIN
                        ALTER TABLE [ManPowers] ADD CONSTRAINT [FK_ManPowers_Services_ServiceId] 
                        FOREIGN KEY([ServiceId]) REFERENCES [Services]([Id]);
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ManPowers");
        }
    }
}
