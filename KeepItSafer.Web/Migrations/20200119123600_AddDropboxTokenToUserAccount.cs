using Microsoft.EntityFrameworkCore.Migrations;

namespace KeepItSafer.Web.Migrations
{
    public partial class AddDropboxTokenToUserAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DropboxToken",
                table: "UserAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropboxToken",
                table: "UserAccounts");
        }
    }
}
