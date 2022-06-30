using Microsoft.EntityFrameworkCore.Migrations;

namespace Aiia.Sample.Data.Migrations
{
    public partial class RenameAccessToken : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("AiiaTokenType",
                                          "AspNetUsers",
                                          "MyDataTokenType");

            migrationBuilder.RenameColumn("AiiaRefreshToken",
                                          "AspNetUsers",
                                          "MyDataRefreshToken");

            migrationBuilder.RenameColumn("AiiaAccessTokenExpires",
                                          "AspNetUsers",
                                          "MyDataAccessTokenExpires");

            migrationBuilder.RenameColumn("AiiaAccessToken",
                                          "AspNetUsers",
                                          "MyDataAccessToken");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("MyDataTokenType",
                                          "AspNetUsers",
                                          "AiiaTokenType");

            migrationBuilder.RenameColumn("MyDataRefreshToken",
                                          "AspNetUsers",
                                          "AiiaRefreshToken");

            migrationBuilder.RenameColumn("MyDataAccessTokenExpires",
                                          "AspNetUsers",
                                          "AiiaAccessTokenExpires");

            migrationBuilder.RenameColumn("MyDataAccessToken",
                                          "AspNetUsers",
                                          "AiiaAccessToken");
        }
    }
}
