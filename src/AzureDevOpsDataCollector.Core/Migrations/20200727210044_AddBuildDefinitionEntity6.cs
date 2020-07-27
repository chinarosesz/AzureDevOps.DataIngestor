using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    public partial class AddBuildDefinitionEntity6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "QueueId",
                table: "VssBuildDefinition",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PoolId",
                table: "VssBuildDefinition",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsHosted",
                table: "VssBuildDefinition",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "RepositoryId",
                table: "VssBuildDefinition",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryName",
                table: "VssBuildDefinition",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "VssBuildDefinition");

            migrationBuilder.DropColumn(
                name: "RepositoryName",
                table: "VssBuildDefinition");

            migrationBuilder.AlterColumn<int>(
                name: "QueueId",
                table: "VssBuildDefinition",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PoolId",
                table: "VssBuildDefinition",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsHosted",
                table: "VssBuildDefinition",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }
    }
}
