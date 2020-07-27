using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddBuildDefinitionEntity3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VssBuildDefinitionStep",
                columns: table => new
                {
                    StepNumber = table.Column<int>(nullable: false),
                    BuildDefinitionId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    Organization = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VssBuildDefinitionStep");
        }
    }
}
