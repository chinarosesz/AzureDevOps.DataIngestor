using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddBuildDefinitionEntity : Migration
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
                    Data = table.Column<string>(nullable: true),
                    RowUpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    ProjectName = table.Column<string>(nullable: true),
                    PoolName = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UniqueName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VssBuildDefinition", x => new { x.Id, x.ProjectId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssBuildDefinition");
        }
    }
}
