using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class HeyIChangedSomething : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VssBuild",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    RepositoryId = table.Column<Guid>(nullable: false),
                    BuildNumber = table.Column<string>(nullable: true),
                    KeepForever = table.Column<bool>(nullable: true),
                    RetainedByRelease = table.Column<bool>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    Result = table.Column<int>(nullable: true),
                    QueueTime = table.Column<DateTime>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: true),
                    FinishTime = table.Column<DateTime>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    DefinitionId = table.Column<int>(nullable: false),
                    SourceBranch = table.Column<string>(nullable: true),
                    SourceVersion = table.Column<string>(nullable: true),
                    QueueId = table.Column<int>(nullable: false),
                    QueueName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssBuild", x => new { x.Id, x.ProjectId });
                });

            migrationBuilder.CreateTable(
                name: "VssBuildWatermarkEntities",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssBuildWatermarkEntities", x => x.ProjectId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssBuild");

            migrationBuilder.DropTable(
                name: "VssBuildWatermarkEntities");
        }
    }
}
