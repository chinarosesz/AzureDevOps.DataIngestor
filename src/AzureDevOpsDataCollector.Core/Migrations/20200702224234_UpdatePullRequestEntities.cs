using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class UpdatePullRequestEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "VssPullRequestWatermark");

            migrationBuilder.DropColumn(
                name: "RepositoryName",
                table: "VssPullRequestWatermark");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark",
                columns: new[] { "PullRequestStatus", "ProjectId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark");

            migrationBuilder.AddColumn<Guid>(
                name: "RepositoryId",
                table: "VssPullRequestWatermark",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RepositoryName",
                table: "VssPullRequestWatermark",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark",
                columns: new[] { "PullRequestStatus", "RepositoryId" });
        }
    }
}
