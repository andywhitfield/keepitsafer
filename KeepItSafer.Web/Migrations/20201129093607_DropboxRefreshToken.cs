using Microsoft.EntityFrameworkCore.Migrations;

namespace KeepItSafer.Web.Migrations
{
    public partial class DropboxRefreshToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DropboxAccessToken",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropboxRefreshToken",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropboxAccessToken",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "DropboxRefreshToken",
                table: "UserAccounts");
        }
    }
}
