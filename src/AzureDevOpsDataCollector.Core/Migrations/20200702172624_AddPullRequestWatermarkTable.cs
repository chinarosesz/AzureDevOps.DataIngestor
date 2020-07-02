using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddPullRequestWatermarkTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VssPullRequestWatermark",
                columns: table => new
                {
                    RepositoryId = table.Column<Guid>(nullable: false),
                    RepositoryName = table.Column<string>(nullable: true),
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    PullRequestStatus = table.Column<byte>(nullable: false),
                    MostRecentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssPullRequestWatermark", x => x.RepositoryId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssPullRequestWatermark");
        }
    }
}
