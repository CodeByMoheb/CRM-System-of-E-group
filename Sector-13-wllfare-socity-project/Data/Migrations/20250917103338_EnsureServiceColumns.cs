using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnsureServiceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ApprovalRequests only if it does not already exist (idempotent for remote DB)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ApprovalRequests')
BEGIN
    CREATE TABLE [ApprovalRequests](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [RequestType] NVARCHAR(MAX) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [RequestedBy] NVARCHAR(MAX) NOT NULL,
        [RequestedByName] NVARCHAR(MAX) NOT NULL,
        [RequestDate] DATETIME2 NOT NULL,
        [Status] INT NOT NULL,
        [SecretaryApprovalBy] NVARCHAR(MAX) NULL,
        [SecretaryApprovalDate] DATETIME2 NULL,
        [SecretaryComments] NVARCHAR(MAX) NULL,
        [PresidentApprovalBy] NVARCHAR(MAX) NULL,
        [PresidentApprovalDate] DATETIME2 NULL,
        [PresidentComments] NVARCHAR(MAX) NULL,
        [RejectionReason] NVARCHAR(MAX) NULL,
        [RejectionDate] DATETIME2 NULL,
        [RejectedBy] NVARCHAR(MAX) NULL
    );
END
");

            // Create AspNetRoles only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [AspNetRoles](
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END
");

            // Create AspNetUsers only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [AspNetUsers](
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [FathersOrHusbandsName] nvarchar(100) NOT NULL,
        [HouseNo] nvarchar(20) NOT NULL,
        [Ward] nvarchar(20) NOT NULL,
        [Holding] nvarchar(20) NOT NULL,
        [Sector] nvarchar(20) NOT NULL,
        [Profession] nvarchar(100) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [BloodGroup] nvarchar(10) NOT NULL,
        [EducationalQualification] nvarchar(100) NOT NULL,
        [NumberOfChildren] int NOT NULL,
        [Telephone] nvarchar(20) NOT NULL,
        [FlatNo] nvarchar(20) NOT NULL,
        [RoadNo] nvarchar(20) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [ProfilePictureUrl] nvarchar(200) NOT NULL,
        [LastLoginTime] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Bookings')
BEGIN
    CREATE TABLE [Bookings](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BookingNumber] NVARCHAR(50) NOT NULL,
        [CustomerName] NVARCHAR(100) NOT NULL,
        [CustomerEmail] NVARCHAR(150) NOT NULL,
        [CustomerPhone] NVARCHAR(20) NOT NULL,
        [CustomerAddress] NVARCHAR(200) NULL,
        [CompanyName] NVARCHAR(200) NULL,
        [SpecialRequirements] NVARCHAR(500) NULL,
        [TotalAmount] DECIMAL(18,2) NOT NULL,
        [Currency] NVARCHAR(10) NOT NULL,
        [PaymentMethod] NVARCHAR(50) NOT NULL,
        [TransactionId] NVARCHAR(MAX) NULL,
        [PaymentStatus] NVARCHAR(20) NOT NULL,
        [BookingDate] DATETIME2 NOT NULL,
        [PaymentDate] DATETIME2 NULL,
        [ServiceDate] DATETIME2 NULL,
        [Notes] NVARCHAR(1000) NULL,
        [BookingStatus] NVARCHAR(50) NULL
    );
END
");

            // Create BuyerContacts only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BuyerContacts')
BEGIN
    CREATE TABLE [BuyerContacts](
        [Id] uniqueidentifier NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [ContactPerson] nvarchar(150) NULL,
        [Designation] nvarchar(100) NULL,
        [Email] nvarchar(200) NULL,
        [Phone] nvarchar(30) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BuyerContacts] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE [Categories](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Type] NVARCHAR(MAX) NULL,
        [Value] NVARCHAR(MAX) NULL,
        [Name] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL,
        [Serial] INT NOT NULL,
        [IsApproved] BIT NOT NULL,
        [IsDelete] BIT NOT NULL,
        [CreatedBy] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [ApprovedBy] NVARCHAR(MAX) NULL,
        [ApprovedAt] DATETIME2 NOT NULL
    );
END
");

            // Create ClientContacts only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClientContacts')
BEGIN
    CREATE TABLE [ClientContacts](
        [Id] uniqueidentifier NOT NULL,
        [Country] nvarchar(100) NULL,
        [Name] nvarchar(200) NULL,
        [Address] nvarchar(300) NULL,
        [ContactPerson] nvarchar(150) NULL,
        [Designation] nvarchar(100) NULL,
        [Email] nvarchar(200) NULL,
        [Phone] nvarchar(30) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ClientContacts] PRIMARY KEY ([Id])
    );
END
");

            // Create ClientsServices only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClientsServices')
BEGIN
    CREATE TABLE [ClientsServices](
        [Id] int IDENTITY(1,1) NOT NULL,
        [Title] nvarchar(max) NULL,
        [ExistingImageUrl] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [IsApproved] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ClientsServices] PRIMARY KEY ([Id])
    );
END
");

            // Create Customers only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE [Customers](
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [HouseNo] nvarchar(max) NOT NULL,
        [Street] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [CompanyName] nvarchar(max) NOT NULL,
        [CompanyCalId] nvarchar(max) NULL,
        [IsApproved] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Donors')
BEGIN
    CREATE TABLE [Donors](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(150) NOT NULL,
        [Phone] NVARCHAR(20) NOT NULL,
        [Address] NVARCHAR(200) NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [PaymentMethod] NVARCHAR(MAX) NOT NULL,
        [TransactionId] NVARCHAR(MAX) NULL,
        [PaymentStatus] NVARCHAR(MAX) NULL,
        [DonationDate] DATETIME2 NOT NULL,
        [Message] NVARCHAR(MAX) NULL,
        [IsAnonymous] BIT NOT NULL,
        [DonationType] NVARCHAR(MAX) NULL,
        [ReceiptNumber] NVARCHAR(MAX) NULL
    );
END
");

            // Create Invoices only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE [Invoices](
        [Id] int IDENTITY(1,1) NOT NULL,
        [InvoiceId] nvarchar(max) NULL,
        [CompanyCalId] nvarchar(max) NULL,
        [InvoiceDate] datetime2 NULL,
        [IsApproved] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id])
    );
END
");

            // Create LocationCharges only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LocationCharges')
BEGIN
    CREATE TABLE [LocationCharges](
        [Id] int IDENTITY(1,1) NOT NULL,
        [LChargeType] nvarchar(max) NULL,
        [LChargeValue] decimal(18,2) NULL,
        [IsApproved] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LocationCharges] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notices')
BEGIN
    CREATE TABLE [Notices](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [CreatedBy] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [IsApproved] BIT NOT NULL,
        [ApprovedBy] NVARCHAR(MAX) NULL,
        [ApprovedAt] DATETIME2 NULL
    );
END
");

            migrationBuilder.Sql(@"
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

            // Create Roles only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE [Roles](
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceCategories')
BEGIN
    CREATE TABLE [ServiceCategories](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IconClass] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL,
        [DisplayOrder] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL
    );
END
");

            // Guarded creation for Shifts to avoid failure if table already exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Shifts')
BEGIN
    CREATE TABLE [Shifts](
        [ShiftId] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [StartTime] TIME NOT NULL,
        [EndTime] TIME NOT NULL,
        [IsActive] BIT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Shifts] PRIMARY KEY ([ShiftId])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [AspNetRoleClaims](
        [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [AspNetUserClaims](
        [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [AspNetUserLogins](
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [AspNetUserRoles](
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [AspNetUserTokens](
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GalleryImages')
BEGIN
    CREATE TABLE [GalleryImages](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(MAX) NOT NULL,
        [CategoryId] INT NOT NULL,
        [ImageUrl] NVARCHAR(MAX) NOT NULL,
        [PublicId] NVARCHAR(MAX) NULL,
        [IsApproved] BIT NOT NULL,
        [IsDelete] BIT NOT NULL,
        [CreatedBy] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [ApprovedBy] NVARCHAR(MAX) NULL,
        [ApprovedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_GalleryImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GalleryImages_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
    );
END
");

            // Guarded creation for Services to avoid failure if table already exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Services')
BEGIN
    CREATE TABLE [Services](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [ServiceType] NVARCHAR(50) NOT NULL,
        [Registration_fees] DECIMAL(18,2) NOT NULL,
        [BasePrice] DECIMAL(18,2) NULL,
        [Currency] NVARCHAR(10) NOT NULL,
        [IsActive] BIT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [ServiceCategoryId] INT NULL,
        [DisplayOrder] INT NOT NULL,
        [CompanyCalId] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Services_ServiceCategories_ServiceCategoryId] FOREIGN KEY ([ServiceCategoryId]) REFERENCES [ServiceCategories]([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE [Employees](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [EmployeeId] NVARCHAR(20) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [RoleId] INT NOT NULL,
        [ShiftId] INT NULL,
        [BaseSalary] DECIMAL(18,2) NOT NULL,
        [JoiningDate] DATETIME2 NOT NULL,
        [Email] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Address] NVARCHAR(200) NULL,
        [PasswordHash] NVARCHAR(100) NULL,
        [PasswordSalt] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Employees_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shifts]([ShiftId])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BookingItems')
BEGIN
    CREATE TABLE [BookingItems](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [BookingId] INT NOT NULL,
        [ServiceId] INT NOT NULL,
        [ServiceName] NVARCHAR(100) NOT NULL,
        [ServiceDescription] NVARCHAR(1000) NULL,
        [ServiceType] NVARCHAR(50) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [Quantity] INT NOT NULL,
        [Subtotal] DECIMAL(18,2) NOT NULL,
        [TravelAllowance] DECIMAL(18,2) NULL,
        [VatAmount] DECIMAL(18,2) NULL,
        [TotalAmount] DECIMAL(18,2) NOT NULL,
        [Currency] NVARCHAR(10) NOT NULL,
        [WorkforceSize] INT NULL,
        [ManDaysRequired] INT NULL,
        [Location] NVARCHAR(50) NULL,
        [ServiceConfiguration] NVARCHAR(2000) NULL,
        CONSTRAINT [PK_BookingItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookingItems_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookingItems_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services]([Id]) ON DELETE CASCADE
    );
END
");

            // ManPowers table will be created by previous migration if needed

            // Attendances table will be created by previous migration if needed

            // Leaves table will be created by previous migration if needed

            // CompanyCals table will be created by previous migration if needed

            // Create indexes conditionally to avoid conflicts
            migrationBuilder.Sql(@"
-- Create indexes only if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID('AspNetRoleClaims'))
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID('AspNetRoles'))
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID('AspNetUserClaims'))
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID('AspNetUserLogins'))
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID('AspNetUserRoles'))
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('AspNetUsers'))
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID('AspNetUsers'))
    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalRequests");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "BookingItems");

            migrationBuilder.DropTable(
                name: "BuyerContacts");

            migrationBuilder.DropTable(
                name: "ClientContacts");

            migrationBuilder.DropTable(
                name: "ClientsServices");

            migrationBuilder.DropTable(
                name: "CompanyCals");

            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropTable(
                name: "GalleryImages");

            migrationBuilder.DropTable(
                name: "Leaves");

            migrationBuilder.DropTable(
                name: "Notices");

            migrationBuilder.DropTable(
                name: "PermanentMembers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "LocationCharges");

            migrationBuilder.DropTable(
                name: "ManPowers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "ServiceCategories");
        }
    }
}
