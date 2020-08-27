using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EntityFramework.BulkOperations.Tests.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeEntities",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(nullable: false),
                    First = table.Column<string>(nullable: true),
                    Last = table.Column<string>(nullable: true),
                    Team = table.Column<string>(nullable: true),
                    Organization = table.Column<string>(nullable: true),
                    HiredDate = table.Column<DateTime>(nullable: false),
                    UniqueName = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeEntities", x => x.EmployeeId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeEntities");
        }
    }
}
