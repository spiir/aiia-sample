using Microsoft.EntityFrameworkCore.Migrations;

namespace ViiaSample.Data.Migrations
{
    public partial class AddViiaConsentId : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                                        "ViiaConsentId",
                                        "AspNetUsers");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                                               "ViiaConsentId",
                                               "AspNetUsers",
                                               nullable: true);
        }
    }
}
