using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VssBuildDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    ProjectName = table.Column<string>(nullable: true),
                    PoolName = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UniqueName = table.Column<string>(nullable: true),
                    Process = table.Column<string>(nullable: true),
                    PoolId = table.Column<int>(nullable: true),
                    IsHosted = table.Column<bool>(nullable: true),
                    QueueName = table.Column<string>(nullable: true),
                    QueueId = table.Column<int>(nullable: true),
                    WebLink = table.Column<string>(nullable: true),
                    RepositoryName = table.Column<string>(nullable: true),
                    RepositoryId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssBuildDefinition", x => new { x.Id, x.ProjectId });
                });

            migrationBuilder.CreateTable(
                name: "VssBuildDefinitionStep",
                columns: table => new
                {
                    StepNumber = table.Column<int>(nullable: false),
                    BuildDefinitionId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    PhaseType = table.Column<string>(nullable: true),
                    PhaseRefName = table.Column<string>(nullable: true),
                    PhaseName = table.Column<string>(nullable: true),
                    PhaseQueueId = table.Column<int>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false),
                    TaskDefinitionId = table.Column<Guid>(nullable: false),
                    TaskVersionSpec = table.Column<string>(nullable: true),
                    Condition = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssBuildDefinitionStep", x => new { x.ProjectId, x.BuildDefinitionId, x.StepNumber });
                });

            migrationBuilder.CreateTable(
                name: "VssProject",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
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
                name: "VssPullRequest",
                columns: table => new
                {
                    PullRequestId = table.Column<int>(nullable: false),
                    RepositoryId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
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

            migrationBuilder.CreateTable(
                name: "VssPullRequestWatermark",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(nullable: false),
                    PullRequestStatus = table.Column<string>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssPullRequestWatermark", x => new { x.PullRequestStatus, x.ProjectId });
                });

            migrationBuilder.CreateTable(
                name: "VssRepository",
                columns: table => new
                {
                    RepoId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DefaultBranch = table.Column<string>(nullable: true),
                    WebUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssRepository", x => x.RepoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VssProject_Organization",
                table: "VssProject",
                column: "Organization")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_VssRepository_Organization",
                table: "VssRepository",
                column: "Organization")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssBuildDefinition");

            migrationBuilder.DropTable(
                name: "VssBuildDefinitionStep");

            migrationBuilder.DropTable(
                name: "VssProject");

            migrationBuilder.DropTable(
                name: "VssPullRequest");

            migrationBuilder.DropTable(
                name: "VssPullRequestWatermark");

            migrationBuilder.DropTable(
                name: "VssRepository");
        }
    }
}
