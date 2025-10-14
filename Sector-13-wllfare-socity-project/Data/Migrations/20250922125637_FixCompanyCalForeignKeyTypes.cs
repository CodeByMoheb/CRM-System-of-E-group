using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCompanyCalForeignKeyTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys and columns only if they exist
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CompanyCals_Customers_CustomerId1', 'F') IS NOT NULL
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Customers_CustomerId1];
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CompanyCals_Invoices_InvoiceId1', 'F') IS NOT NULL
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Invoices_InvoiceId1];
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CompanyCals_LocationCharges_LocationChargeId', 'F') IS NOT NULL
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_LocationCharges_LocationChargeId];
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CompanyCals_ManPowers_ManPowerId', 'F') IS NOT NULL
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_ManPowers_ManPowerId];
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CompanyCals_Services_ServiceId', 'F') IS NOT NULL
                    ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Services_ServiceId];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CompanyCals_CustomerId1' AND object_id = OBJECT_ID('CompanyCals'))
                    DROP INDEX [IX_CompanyCals_CustomerId1] ON [CompanyCals];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CompanyCals_InvoiceId1' AND object_id = OBJECT_ID('CompanyCals'))
                    DROP INDEX [IX_CompanyCals_InvoiceId1] ON [CompanyCals];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND name = 'CustomerId1')
                    ALTER TABLE [CompanyCals] DROP COLUMN [CustomerId1];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CompanyCals]') AND name = 'InvoiceId1')
                    ALTER TABLE [CompanyCals] DROP COLUMN [InvoiceId1];
            ");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "CompanyCals",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "CompanyCals",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCals_CustomerId",
                table: "CompanyCals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCals_InvoiceId",
                table: "CompanyCals",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_Customers_CustomerId",
                table: "CompanyCals",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_Invoices_InvoiceId",
                table: "CompanyCals",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_LocationCharges_LocationChargeId",
                table: "CompanyCals",
                column: "LocationChargeId",
                principalTable: "LocationCharges",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_ManPowers_ManPowerId",
                table: "CompanyCals",
                column: "ManPowerId",
                principalTable: "ManPowers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_Services_ServiceId",
                table: "CompanyCals",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyCals_Customers_CustomerId",
                table: "CompanyCals");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyCals_Invoices_InvoiceId",
                table: "CompanyCals");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyCals_LocationCharges_LocationChargeId",
                table: "CompanyCals");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyCals_ManPowers_ManPowerId",
                table: "CompanyCals");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyCals_Services_ServiceId",
                table: "CompanyCals");

            migrationBuilder.DropIndex(
                name: "IX_CompanyCals_CustomerId",
                table: "CompanyCals");

            migrationBuilder.DropIndex(
                name: "IX_CompanyCals_InvoiceId",
                table: "CompanyCals");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceId",
                table: "CompanyCals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "CompanyCals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                table: "CompanyCals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId1",
                table: "CompanyCals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCals_CustomerId1",
                table: "CompanyCals",
                column: "CustomerId1");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCals_InvoiceId1",
                table: "CompanyCals",
                column: "InvoiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_Customers_CustomerId1",
                table: "CompanyCals",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_Invoices_InvoiceId1",
                table: "CompanyCals",
                column: "InvoiceId1",
                principalTable: "Invoices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_LocationCharges_LocationChargeId",
                table: "CompanyCals",
                column: "LocationChargeId",
                principalTable: "LocationCharges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_ManPowers_ManPowerId",
                table: "CompanyCals",
                column: "ManPowerId",
                principalTable: "ManPowers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyCals_Services_ServiceId",
                table: "CompanyCals",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }
    }
}
