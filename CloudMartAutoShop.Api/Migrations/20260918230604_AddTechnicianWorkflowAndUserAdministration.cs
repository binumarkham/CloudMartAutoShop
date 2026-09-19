using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudMartAutoShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianWorkflowAndUserAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedTechnicianName",
                table: "RepairOrders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedTechnicianUserId",
                table: "RepairOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianNotes",
                table: "RepairOrderLabors",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTechnicianName",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianUserId",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianNotes",
                table: "RepairOrderLabors");
        }
    }
}
