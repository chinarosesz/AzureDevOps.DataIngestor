using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class RevertUpdateEntityWatermarkPullRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MostRecentPullRequestDate",
                table: "VssPullRequestWatermark");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MostRecentPullRequestDate",
                table: "VssPullRequestWatermark",
                type: "datetime2",
                nullable: true);
        }
    }
}
