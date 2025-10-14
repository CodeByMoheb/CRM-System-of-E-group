using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToLeaveEntitlementPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CarryForwardEnabled",
                table: "LeaveEntitlementPolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxCarryForward",
                table: "LeaveEntitlementPolicies",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarryForwardEnabled",
                table: "LeaveEntitlementPolicies");

            migrationBuilder.DropColumn(
                name: "MaxCarryForward",
                table: "LeaveEntitlementPolicies");
        }
    }
}
