using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class InitialCreate3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "VssBuildDefinition");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "VssBuildDefinition",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
