using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class Auto_2025_09_17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent drops: only drop if the FK/index/column exists
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Customers_CustomerId1')
    ALTER TABLE [dbo].[CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Customers_CustomerId1];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Invoices_InvoiceId1')
    ALTER TABLE [dbo].[CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Invoices_InvoiceId1];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Roles_RoleId1')
    ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [FK_Employees_Roles_RoleId1];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_RoleId1' AND object_id = OBJECT_ID('[dbo].[Employees]'))
    DROP INDEX [IX_Employees_RoleId1] ON [dbo].[Employees];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CompanyCals_CustomerId1' AND object_id = OBJECT_ID('[dbo].[CompanyCals]'))
    DROP INDEX [IX_CompanyCals_CustomerId1] ON [dbo].[CompanyCals];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CompanyCals_InvoiceId1' AND object_id = OBJECT_ID('[dbo].[CompanyCals]'))
    DROP INDEX [IX_CompanyCals_InvoiceId1] ON [dbo].[CompanyCals];

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'RoleId1' AND Object_ID = OBJECT_ID('[dbo].[Employees]'))
    ALTER TABLE [dbo].[Employees] DROP COLUMN [RoleId1];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'CustomerId1' AND Object_ID = OBJECT_ID('[dbo].[CompanyCals]'))
    ALTER TABLE [dbo].[CompanyCals] DROP COLUMN [CustomerId1];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'InvoiceId1' AND Object_ID = OBJECT_ID('[dbo].[CompanyCals]'))
    ALTER TABLE [dbo].[CompanyCals] DROP COLUMN [InvoiceId1];

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId1",
                table: "Employees",
                type: "int",
                nullable: true);

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
                name: "IX_Employees_RoleId1",
                table: "Employees",
                column: "RoleId1");

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
                name: "FK_Employees_Roles_RoleId1",
                table: "Employees",
                column: "RoleId1",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
