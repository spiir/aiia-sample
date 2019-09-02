using Microsoft.EntityFrameworkCore.Migrations;

namespace ViiaSample.Data.Migrations
{
    public partial class AddEmailEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailEnabled",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailEnabled",
                table: "AspNetUsers");
        }
    }
}
