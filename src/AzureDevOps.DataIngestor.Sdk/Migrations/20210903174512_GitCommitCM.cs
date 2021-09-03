using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class GitCommitCM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LatestBuildFinishTime",
                table: "VssBuildWatermarkEntities",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "VssCommit",
                columns: table => new
                {
                    CommitId = table.Column<string>(nullable: false),
                    RepositoryId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    AuthorEmail = table.Column<string>(maxLength: 300, nullable: true),
                    CommitTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(maxLength: 500, nullable: true),
                    RemoteUrl = table.Column<string>(maxLength: 2083, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssCommit", x => new { x.CommitId, x.RepositoryId });
                });

            migrationBuilder.CreateTable(
                name: "VssCommitWatermark",
                columns: table => new
                {
                    RepositoryId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    ProjectId = table.Column<Guid>(nullable: false),
                    ProjectName = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssCommitWatermark", x => x.RepositoryId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssCommit");

            migrationBuilder.DropTable(
                name: "VssCommitWatermark");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LatestBuildFinishTime",
                table: "VssBuildWatermarkEntities",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime));
        }
    }
}
