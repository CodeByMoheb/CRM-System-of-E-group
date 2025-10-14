using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class BookingUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Shifts_ShiftId",
                table: "Employees");

            // Guarded drop: Services.PricingType may already be removed by prior migration
            migrationBuilder.Sql(@"IF COL_LENGTH('Services','PricingType') IS NOT NULL
BEGIN
    DECLARE @dc1 sysname;
    SELECT @dc1 = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('Services') AND c.name = 'PricingType';
    IF @dc1 IS NOT NULL EXEC('ALTER TABLE [Services] DROP CONSTRAINT [' + @dc1 + ']');
    ALTER TABLE [Services] DROP COLUMN [PricingType];
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('GalleryImages','Category') IS NOT NULL
BEGIN
    DECLARE @dcGI sysname;
    SELECT @dcGI = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('GalleryImages') AND c.name = 'Category';
    IF @dcGI IS NOT NULL EXEC('ALTER TABLE [GalleryImages] DROP CONSTRAINT [' + @dcGI + ']');
    ALTER TABLE [GalleryImages] DROP COLUMN [Category];
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('Employees','Category') IS NOT NULL
BEGIN
    DECLARE @dcEmpCat sysname;
    SELECT @dcEmpCat = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('Employees') AND c.name = 'Category';
    IF @dcEmpCat IS NOT NULL EXEC('ALTER TABLE [Employees] DROP CONSTRAINT [' + @dcEmpCat + ']');
    ALTER TABLE [Employees] DROP COLUMN [Category];
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('Employees','Role') IS NOT NULL
BEGIN
    DECLARE @dcEmpRole sysname;
    SELECT @dcEmpRole = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('Employees') AND c.name = 'Role';
    IF @dcEmpRole IS NOT NULL EXEC('ALTER TABLE [Employees] DROP CONSTRAINT [' + @dcEmpRole + ']');
    ALTER TABLE [Employees] DROP COLUMN [Role];
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('Bookings','UpdatedAt') IS NOT NULL
BEGIN
    DECLARE @dc2 sysname;
    SELECT @dc2 = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('Bookings') AND c.name = 'UpdatedAt';
    IF @dc2 IS NOT NULL EXEC('ALTER TABLE [Bookings] DROP CONSTRAINT [' + @dc2 + ']');
    ALTER TABLE [Bookings] DROP COLUMN [UpdatedAt];
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('BookingItems','PricingType') IS NOT NULL
BEGIN
    DECLARE @dc3 sysname;
    SELECT @dc3 = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('BookingItems') AND c.name = 'PricingType';
    IF @dc3 IS NOT NULL EXEC('ALTER TABLE [BookingItems] DROP CONSTRAINT [' + @dc3 + ']');
    ALTER TABLE [BookingItems] DROP COLUMN [PricingType];
END");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "GalleryImages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "GalleryImages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedAt",
                table: "GalleryImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // Guard Add CategoryId
            migrationBuilder.Sql(@"IF COL_LENGTH('GalleryImages','CategoryId') IS NULL
BEGIN
    ALTER TABLE [GalleryImages] ADD [CategoryId] int NOT NULL DEFAULT 0;
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('GalleryImages','IsDelete') IS NULL
BEGIN
    ALTER TABLE [GalleryImages] ADD [IsDelete] bit NOT NULL DEFAULT 0;
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('Employees','RoleId') IS NULL
BEGIN
    ALTER TABLE [Employees] ADD [RoleId] int NOT NULL DEFAULT 0;
END");

            migrationBuilder.Sql(@"IF COL_LENGTH('Employees','RoleId1') IS NULL
BEGIN
    ALTER TABLE [Employees] ADD [RoleId1] int NULL;
END");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'Categories' AND type = 'U')
BEGIN
    CREATE TABLE [Categories](
        [Id] int NOT NULL IDENTITY(1,1),
        [Type] nvarchar(max) NULL,
        [Value] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [Serial] int NOT NULL,
        [IsApproved] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END");

            // Create indexes/foreign keys if missing
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GalleryImages_CategoryId' AND object_id = OBJECT_ID('GalleryImages'))
BEGIN
    CREATE INDEX [IX_GalleryImages_CategoryId] ON [GalleryImages]([CategoryId]);
END");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_RoleId' AND object_id = OBJECT_ID('Employees'))
BEGIN
    CREATE INDEX [IX_Employees_RoleId] ON [Employees]([RoleId]);
END");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_RoleId1' AND object_id = OBJECT_ID('Employees'))
BEGIN
    CREATE INDEX [IX_Employees_RoleId1] ON [Employees]([RoleId1]);
END");

            // Create Roles table if it doesn't exist (must be before foreign key creation)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Roles] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [Name] nvarchar(100) NOT NULL,
                        [Description] nvarchar(500) NULL,
                        [IsActive] bit NOT NULL DEFAULT 1,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
                    );
                    
                    -- Insert some default roles
                    INSERT INTO [dbo].[Roles] ([Name], [Description], [IsActive], [CreatedAt])
                    VALUES 
                        ('Office Manager', 'Manages office operations and staff', 1, GETUTCDATE()),
                        ('Security Guard', 'Provides security and safety services', 1, GETUTCDATE()),
                        ('Cleaner', 'Maintains cleanliness and hygiene', 1, GETUTCDATE()),
                        ('Driver', 'Provides transportation services', 1, GETUTCDATE()),
                        ('Receptionist', 'Handles front desk and customer service', 1, GETUTCDATE());
                END
            ");

            // Update any invalid RoleId values before adding foreign key constraint
            migrationBuilder.Sql(@"UPDATE [Employees] SET [RoleId] = 1 WHERE [RoleId] NOT IN (SELECT [Id] FROM [Roles])");
            
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Roles_RoleId')
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_Roles_RoleId] FOREIGN KEY([RoleId]) REFERENCES [Roles]([Id]) ON DELETE NO ACTION;
END");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Roles_RoleId1')
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_Roles_RoleId1] FOREIGN KEY([RoleId1]) REFERENCES [Roles]([Id]);
END");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Shifts_ShiftId')
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_Shifts_ShiftId] FOREIGN KEY([ShiftId]) REFERENCES [Shifts]([ShiftId]) ON DELETE SET NULL;
END");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GalleryImages_Categories_CategoryId')
BEGIN
    ALTER TABLE [GalleryImages] ADD CONSTRAINT [FK_GalleryImages_Categories_CategoryId] FOREIGN KEY([CategoryId]) REFERENCES [Categories]([Id]) ON DELETE CASCADE;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Roles_RoleId1",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Shifts_ShiftId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryImages_Categories_CategoryId",
                table: "GalleryImages");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_GalleryImages_CategoryId",
                table: "GalleryImages");

            migrationBuilder.DropIndex(
                name: "IX_Employees_RoleId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_RoleId1",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "PricingType",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "GalleryImages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "GalleryImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedAt",
                table: "GalleryImages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "GalleryImages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricingType",
                table: "BookingItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Shifts_ShiftId",
                table: "Employees",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "ShiftId");
        }
    }
}
