using Microsoft.EntityFrameworkCore.Migrations;

namespace Aiia.Sample.Data.Migrations
{
    public partial class AddEmailEnabled : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("EmailEnabled",
                                        "AspNetUsers");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("EmailEnabled",
                                             "AspNetUsers",
                                             nullable: false,
                                             defaultValue: true);
        }
    }
}
