using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class UpdateColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exists",
                table: "VssRepository");

            migrationBuilder.DropColumn(
                name: "RepoName",
                table: "VssRepository");

            migrationBuilder.DropColumn(
                name: "Exists",
                table: "VssProject");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "VssRepository",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "VssRepository");

            migrationBuilder.AddColumn<bool>(
                name: "Exists",
                table: "VssRepository",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RepoName",
                table: "VssRepository",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Exists",
                table: "VssProject",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
