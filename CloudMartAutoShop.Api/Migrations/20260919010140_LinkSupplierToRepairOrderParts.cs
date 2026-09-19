using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CloudMartAutoShop.Api.Data;

#nullable disable

namespace CloudMartAutoShop.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260919010140_LinkSupplierToRepairOrderParts")]
    public partial class LinkSupplierToRepairOrderParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "RepairOrderParts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderParts_BusinessId_SupplierId",
                table: "RepairOrderParts",
                columns: new[] { "BusinessId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderParts_SupplierId",
                table: "RepairOrderParts",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderParts_Suppliers_SupplierId",
                table: "RepairOrderParts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_RepairOrderParts_Suppliers_SupplierId", table: "RepairOrderParts");
            migrationBuilder.DropIndex(name: "IX_RepairOrderParts_BusinessId_SupplierId", table: "RepairOrderParts");
            migrationBuilder.DropIndex(name: "IX_RepairOrderParts_SupplierId", table: "RepairOrderParts");
            migrationBuilder.DropColumn(name: "SupplierId", table: "RepairOrderParts");
        }
    }
}
