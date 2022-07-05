using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aiia.Sample.Data.Migrations
{
    public partial class AddWebhooksStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Webhooks", columns =>
                new
                {
                    Id = columns.Column<ulong>(),
                    Received = columns.Column<DateTime>(),
                    UserId = columns.Column<Guid>(),
                    DataAsJson = columns.Column<string>()
                }, constraints: constraints =>
            {
                constraints.PrimaryKey("PK_Id", columns: x => x.Id);
            });
            
            migrationBuilder.CreateIndex("IDX_Webhooks_UserId", "Webhooks", "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IDX_Webhooks_UserId");
            migrationBuilder.DropTable("Webhooks");
        }
    }
}
