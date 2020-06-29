using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddNewColumnExistsForProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrganizationName",
                table: "VssRepository",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationName",
                table: "VssProject",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Exists",
                table: "VssProject",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_VssRepository_OrganizationName",
                table: "VssRepository",
                column: "OrganizationName")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_VssProject_OrganizationName",
                table: "VssProject",
                column: "OrganizationName")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VssRepository_OrganizationName",
                table: "VssRepository");

            migrationBuilder.DropIndex(
                name: "IX_VssProject_OrganizationName",
                table: "VssProject");

            migrationBuilder.DropColumn(
                name: "Exists",
                table: "VssProject");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationName",
                table: "VssRepository",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationName",
                table: "VssProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
