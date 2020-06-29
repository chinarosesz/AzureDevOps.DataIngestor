using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddNewColumnExistsRepositoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Exists",
                table: "VssRepository",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exists",
                table: "VssRepository");
        }
    }
}
