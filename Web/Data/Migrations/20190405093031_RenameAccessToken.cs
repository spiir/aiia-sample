using Microsoft.EntityFrameworkCore.Migrations;

namespace ViiaSample.Data.Migrations
{
    public partial class RenameAccessToken : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                                          "ViiaTokenType",
                                          "AspNetUsers",
                                          "MyDataTokenType");

            migrationBuilder.RenameColumn(
                                          "ViiaRefreshToken",
                                          "AspNetUsers",
                                          "MyDataRefreshToken");

            migrationBuilder.RenameColumn(
                                          "ViiaAccessTokenExpires",
                                          "AspNetUsers",
                                          "MyDataAccessTokenExpires");

            migrationBuilder.RenameColumn(
                                          "ViiaAccessToken",
                                          "AspNetUsers",
                                          "MyDataAccessToken");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                                          "MyDataTokenType",
                                          "AspNetUsers",
                                          "ViiaTokenType");

            migrationBuilder.RenameColumn(
                                          "MyDataRefreshToken",
                                          "AspNetUsers",
                                          "ViiaRefreshToken");

            migrationBuilder.RenameColumn(
                                          "MyDataAccessTokenExpires",
                                          "AspNetUsers",
                                          "ViiaAccessTokenExpires");

            migrationBuilder.RenameColumn(
                                          "MyDataAccessToken",
                                          "AspNetUsers",
                                          "ViiaAccessToken");
        }
    }
}
