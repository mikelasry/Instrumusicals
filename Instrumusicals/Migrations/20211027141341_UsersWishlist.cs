using Microsoft.EntityFrameworkCore.Migrations;

namespace Instrumusicals.Migrations
{
    public partial class UsersWishlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstrumentsWishlist",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstrumentsWishlist",
                table: "User");
        }
    }
}
