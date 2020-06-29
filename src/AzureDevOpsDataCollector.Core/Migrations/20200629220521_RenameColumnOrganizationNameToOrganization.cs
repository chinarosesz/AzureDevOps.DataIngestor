using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class RenameColumnOrganizationNameToOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VssRepository_OrganizationName",
                table: "VssRepository");

            migrationBuilder.DropIndex(
                name: "IX_VssProject_OrganizationName",
                table: "VssProject");

            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "VssRepository");

            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "VssProject");

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "VssRepository",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "VssProject",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VssRepository_Organization",
                table: "VssRepository");

            migrationBuilder.DropIndex(
                name: "IX_VssProject_Organization",
                table: "VssProject");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "VssRepository");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "VssProject");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "VssRepository",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "VssProject",
                type: "nvarchar(450)",
                nullable: true);

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
    }
}
