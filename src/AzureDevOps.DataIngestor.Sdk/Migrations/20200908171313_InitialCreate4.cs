using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class InitialCreate4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VssRepository_Organization",
                table: "VssRepository");

            migrationBuilder.DropIndex(
                name: "IX_VssProject_Organization",
                table: "VssProject");

            migrationBuilder.CreateTable(
                name: "VssDataEntities",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssDataEntities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VssRepository_Organization",
                table: "VssRepository",
                column: "Organization");

            migrationBuilder.CreateIndex(
                name: "IX_VssProject_Organization",
                table: "VssProject",
                column: "Organization");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssDataEntities");

            migrationBuilder.DropIndex(
                name: "IX_VssRepository_Organization",
                table: "VssRepository");

            migrationBuilder.DropIndex(
                name: "IX_VssProject_Organization",
                table: "VssProject");

            migrationBuilder.CreateIndex(
                name: "IX_VssRepository_Organization",
                table: "VssRepository",
                column: "Organization")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_VssProject_Organization",
                table: "VssProject",
                column: "Organization")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
