using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class UpdatePullRequestWatermarkEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MostRecentDate",
                table: "VssPullRequestWatermark");

            migrationBuilder.DropColumn(
                name: "PullRequestId",
                table: "VssPullRequestWatermark");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MostRecentDate",
                table: "VssPullRequestWatermark",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PullRequestId",
                table: "VssPullRequestWatermark",
                type: "int",
                nullable: true);
        }
    }
}
