using Microsoft.EntityFrameworkCore.Migrations;

namespace ViiaSample.Data.Migrations
{
    public partial class AddViiaConsentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ViiaConsentId",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViiaConsentId",
                table: "AspNetUsers");
        }
    }
}
