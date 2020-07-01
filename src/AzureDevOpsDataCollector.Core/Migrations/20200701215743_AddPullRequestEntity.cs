using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddPullRequestEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VssPullRequest",
                columns: table => new
                {
                    PullRequestId = table.Column<int>(nullable: false),
                    RepositoryId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    AuthorEmail = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false),
                    LastMergeCommitID = table.Column<string>(nullable: true),
                    SourceBranch = table.Column<string>(nullable: false),
                    LastMergeTargetCommitId = table.Column<string>(nullable: true),
                    TargetBranch = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssPullRequest", x => new { x.PullRequestId, x.RepositoryId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssPullRequest");
        }
    }
}
