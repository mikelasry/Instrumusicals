using Microsoft.EntityFrameworkCore.Migrations;

namespace Instrumusicals.Migrations
{
    public partial class OrderWishlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderWishlist",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderWishlist",
                table: "Order");
        }
    }
}
