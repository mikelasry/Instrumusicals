using Microsoft.EntityFrameworkCore.Migrations;

namespace Instrumusicals.Migrations
{
    public partial class OrderShipping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "Order",
                newName: "Shipping");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Shipping",
                table: "Order",
                newName: "LastUpdate");
        }
    }
}
