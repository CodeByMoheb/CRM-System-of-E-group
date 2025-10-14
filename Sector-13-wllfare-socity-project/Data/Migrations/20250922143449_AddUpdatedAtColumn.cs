using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if UpdatedAt column already exists before adding it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'UpdatedAt')
                    ALTER TABLE [Orders] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OrderItems]') AND name = 'UpdatedAt')
                    ALTER TABLE [OrderItems] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ManPowers]') AND name = 'UpdatedAt')
                    ALTER TABLE [ManPowers] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LocationCharges]') AND name = 'UpdatedAt')
                    ALTER TABLE [LocationCharges] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'UpdatedAt')
                    ALTER TABLE [Invoices] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GalleryImages]') AND name = 'UpdatedAt')
                    ALTER TABLE [GalleryImages] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'UpdatedAt')
                    ALTER TABLE [Customers] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND name = 'UpdatedAt')
                    ALTER TABLE [CompanyCals] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ClientsServices]') AND name = 'UpdatedAt')
                    ALTER TABLE [ClientsServices] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND name = 'UpdatedAt')
                    ALTER TABLE [Categories] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND name = 'UpdatedAt')
                    ALTER TABLE [CartItems] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
            ");

            migrationBuilder.CreateTable(
                name: "PaymentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentProofUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_OrderId",
                table: "PaymentRecords",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ManPowers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LocationCharges");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CompanyCals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClientsServices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CartItems");
        }
    }
}
