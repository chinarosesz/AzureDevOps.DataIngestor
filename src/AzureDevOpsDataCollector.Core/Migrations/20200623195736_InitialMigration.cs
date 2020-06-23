using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VssProject",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(nullable: false),
                    OrganizationName = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Revision = table.Column<long>(nullable: false),
                    Visibility = table.Column<string>(nullable: true),
                    LastUpdateTime = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssProject", x => x.ProjectId);
                });

            migrationBuilder.CreateTable(
                name: "VssRepository",
                columns: table => new
                {
                    RepoId = table.Column<Guid>(nullable: false),
                    OrganizationName = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    RepoName = table.Column<string>(nullable: true),
                    DefaultBranch = table.Column<string>(nullable: true),
                    WebUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssRepository", x => x.RepoId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssProject");

            migrationBuilder.DropTable(
                name: "VssRepository");
        }
    }
}
