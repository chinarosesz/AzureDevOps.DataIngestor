using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class UpdatePullRequestWatermarkEntity2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark");

            migrationBuilder.AlterColumn<string>(
                name: "PullRequestStatus",
                table: "VssPullRequestWatermark",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark",
                columns: new[] { "PullRequestStatus", "RepositoryId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark");

            migrationBuilder.AlterColumn<string>(
                name: "PullRequestStatus",
                table: "VssPullRequestWatermark",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_VssPullRequestWatermark",
                table: "VssPullRequestWatermark",
                column: "RepositoryId");
        }
    }
}
