using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    public partial class NewBuildWatermarkColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LatestBuildFinishTime",
                table: "VssBuildWatermarkEntities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatestBuildFinishTime",
                table: "VssBuildWatermarkEntities");
        }
    }
}
