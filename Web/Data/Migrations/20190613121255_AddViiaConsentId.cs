using Microsoft.EntityFrameworkCore.Migrations;

namespace Aiia.Sample.Data.Migrations
{
    public partial class AddAiiaConsentId : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                                        "AiiaConsentId",
                                        "AspNetUsers");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                                               "AiiaConsentId",
                                               "AspNetUsers",
                                               nullable: true);
        }
    }
}
