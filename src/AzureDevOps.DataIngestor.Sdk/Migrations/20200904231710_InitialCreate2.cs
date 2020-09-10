using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "VssBuildDefinition",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "VssBuildDefinition");
        }
    }
}
