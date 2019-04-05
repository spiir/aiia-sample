using Microsoft.EntityFrameworkCore.Migrations;

namespace ViiaSample.Data.Migrations
{
    public partial class RenameAccessToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyDataTokenType",
                table: "AspNetUsers",
                newName: "ViiaTokenType");

            migrationBuilder.RenameColumn(
                name: "MyDataRefreshToken",
                table: "AspNetUsers",
                newName: "ViiaRefreshToken");

            migrationBuilder.RenameColumn(
                name: "MyDataAccessTokenExpires",
                table: "AspNetUsers",
                newName: "ViiaAccessTokenExpires");

            migrationBuilder.RenameColumn(
                name: "MyDataAccessToken",
                table: "AspNetUsers",
                newName: "ViiaAccessToken");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ViiaTokenType",
                table: "AspNetUsers",
                newName: "MyDataTokenType");

            migrationBuilder.RenameColumn(
                name: "ViiaRefreshToken",
                table: "AspNetUsers",
                newName: "MyDataRefreshToken");

            migrationBuilder.RenameColumn(
                name: "ViiaAccessTokenExpires",
                table: "AspNetUsers",
                newName: "MyDataAccessTokenExpires");

            migrationBuilder.RenameColumn(
                name: "ViiaAccessToken",
                table: "AspNetUsers",
                newName: "MyDataAccessToken");
        }
    }
}
