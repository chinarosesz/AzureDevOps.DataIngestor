using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddBuildDefinitionEntity1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHosted",
                table: "VssBuildDefinition",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PoolId",
                table: "VssBuildDefinition",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Process",
                table: "VssBuildDefinition",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueId",
                table: "VssBuildDefinition",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QueueName",
                table: "VssBuildDefinition",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHosted",
                table: "VssBuildDefinition");

            migrationBuilder.DropColumn(
                name: "PoolId",
                table: "VssBuildDefinition");

            migrationBuilder.DropColumn(
                name: "Process",
                table: "VssBuildDefinition");

            migrationBuilder.DropColumn(
                name: "QueueId",
                table: "VssBuildDefinition");

            migrationBuilder.DropColumn(
                name: "QueueName",
                table: "VssBuildDefinition");
        }
    }
}
