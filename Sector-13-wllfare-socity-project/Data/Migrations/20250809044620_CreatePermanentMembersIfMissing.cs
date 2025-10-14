using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatePermanentMembersIfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('[dbo].[SheetMembers]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[SheetMembers];
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermanentMembers')
BEGIN
    CREATE TABLE [PermanentMembers](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [FathersOrHusbandsName] NVARCHAR(100) NOT NULL,
        [Address] NVARCHAR(200) NOT NULL,
        [RoadNo] NVARCHAR(20) NOT NULL,
        [HouseNo] NVARCHAR(20) NOT NULL,
        [Sector] NVARCHAR(20) NOT NULL,
        [PhoneNumber] NVARCHAR(15) NOT NULL,
        [Email] NVARCHAR(100) NULL,
        [DateOfBirth] DATETIME2 NULL,
        [NationalId] NVARCHAR(20) NULL,
        [MembershipDate] DATETIME2 NOT NULL,
        [IsActive] BIT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [Notes] NVARCHAR(500) NULL
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermanentMembers");

            migrationBuilder.CreateTable(
                name: "SheetMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetMembers", x => x.Id);
                });
        }
    }
}
