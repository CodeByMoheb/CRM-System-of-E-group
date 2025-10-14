using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeBookingItemServiceOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_Services_ServiceId",
                table: "BookingItems");

            migrationBuilder.Sql(@"
DECLARE @schema sysname;
SELECT TOP(1) @schema = s.name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name = 'CompanyCals';

IF @schema IS NOT NULL
BEGIN
    DECLARE @qualified nvarchar(400) = QUOTENAME(@schema) + '.[CompanyCals]';
    DECLARE @sql nvarchar(max) = N'IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @qualified + N''') AND name = ''Status'')
BEGIN
    ALTER TABLE ' + @qualified + N' ADD [Status] nvarchar(max) NULL;
END';
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'CompanyCals table not found; skipped Status column creation.';
END");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "BookingItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "AuditQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditQuestions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    AuditorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuditStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresCAP = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditSessions_AspNetUsers_AuditorId",
                        column: x => x.AuditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditSessions_CompanyCals_BookingId",
                        column: x => x.BookingId,
                        principalTable: "CompanyCals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditQuestionId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ResponseValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditSessionId = table.Column<int>(type: "int", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditResponses_AspNetUsers_AuditorId",
                        column: x => x.AuditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditResponses_AuditQuestions_AuditQuestionId",
                        column: x => x.AuditQuestionId,
                        principalTable: "AuditQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditResponses_AuditSessions_AuditSessionId",
                        column: x => x.AuditSessionId,
                        principalTable: "AuditSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditResponses_CompanyCals_BookingId",
                        column: x => x.BookingId,
                        principalTable: "CompanyCals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorrectiveActionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditSessionId = table.Column<int>(type: "int", nullable: false),
                    AuditResponseId = table.Column<int>(type: "int", nullable: false),
                    IssueDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActionPlans_AuditResponses_AuditResponseId",
                        column: x => x.AuditResponseId,
                        principalTable: "AuditResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CorrectiveActionPlans_AuditSessions_AuditSessionId",
                        column: x => x.AuditSessionId,
                        principalTable: "AuditSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditQuestions_ServiceId",
                table: "AuditQuestions",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditorId",
                table: "AuditResponses",
                column: "AuditorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditQuestionId",
                table: "AuditResponses",
                column: "AuditQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditSessionId",
                table: "AuditResponses",
                column: "AuditSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_BookingId",
                table: "AuditResponses",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditSessions_AuditorId",
                table: "AuditSessions",
                column: "AuditorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditSessions_BookingId",
                table: "AuditSessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionPlans_AuditResponseId",
                table: "CorrectiveActionPlans",
                column: "AuditResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionPlans_AuditSessionId",
                table: "CorrectiveActionPlans",
                column: "AuditSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_Services_ServiceId",
                table: "BookingItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_Services_ServiceId",
                table: "BookingItems");

            migrationBuilder.DropTable(
                name: "CorrectiveActionPlans");

            migrationBuilder.DropTable(
                name: "AuditResponses");

            migrationBuilder.DropTable(
                name: "AuditQuestions");

            migrationBuilder.DropTable(
                name: "AuditSessions");

            migrationBuilder.Sql(@"
DECLARE @schema sysname;
SELECT TOP(1) @schema = s.name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name = 'CompanyCals';

IF @schema IS NOT NULL
BEGIN
    DECLARE @qualified nvarchar(400) = QUOTENAME(@schema) + '.[CompanyCals]';
    DECLARE @sql nvarchar(max) = N'IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @qualified + N''') AND name = ''Status'')
BEGIN
    ALTER TABLE ' + @qualified + N' DROP COLUMN [Status];
END';
    EXEC sp_executesql @sql;
END");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "BookingItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_Services_ServiceId",
                table: "BookingItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
