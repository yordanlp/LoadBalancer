using Microsoft.EntityFrameworkCore.Migrations;

namespace LoadBalancer.Migrations
{
    public partial class ChangedInstancestructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "host",
                table: "Instances",
                newName: "InternalHost");

            migrationBuilder.AddColumn<string>(
                name: "ExternalHost",
                table: "Instances",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalHost",
                table: "Instances");

            migrationBuilder.RenameColumn(
                name: "InternalHost",
                table: "Instances",
                newName: "host");
        }
    }
}
