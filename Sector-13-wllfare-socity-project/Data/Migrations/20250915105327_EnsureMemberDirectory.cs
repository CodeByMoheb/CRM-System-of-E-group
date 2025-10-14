using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnsureMemberDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.BuyerContacts','U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BuyerContacts](
        [Id] uniqueidentifier NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [ContactPerson] nvarchar(150) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BuyerContacts] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID('dbo.ClientContacts','U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ClientContacts](
        [Id] uniqueidentifier NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [ContactPerson] nvarchar(150) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ClientContacts] PRIMARY KEY ([Id])
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.BuyerContacts','U') IS NOT NULL DROP TABLE [dbo].[BuyerContacts];");
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.ClientContacts','U') IS NOT NULL DROP TABLE [dbo].[ClientContacts];");
        }
    }
}
