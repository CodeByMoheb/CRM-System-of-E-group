﻿﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateLocationChargesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only create the LocationCharges table if it doesn't exist
            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only drop the LocationCharges table if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='LocationCharges' AND xtype='U')
                BEGIN
                    DROP TABLE [dbo].[LocationCharges];
                    PRINT 'LocationCharges table dropped successfully.';
                END
                ELSE
                BEGIN
                    PRINT 'LocationCharges table does not exist.';
                END
            ");
        }
    }
}
