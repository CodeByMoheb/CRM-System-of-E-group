﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateRolesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles table should already be created by earlier migration (20250915094219_BookingUpdates)
            // This migration is kept for consistency but does nothing
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Roles table if it exists and has no foreign key dependencies
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
                BEGIN
                    -- First check if there are any foreign key constraints referencing this table
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID(N'[dbo].[Roles]'))
                    BEGIN
                        DROP TABLE [dbo].[Roles];
                    END
                END
            ");
        }
    }
}
