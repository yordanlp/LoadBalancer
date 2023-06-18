using Microsoft.EntityFrameworkCore.Migrations;

namespace LoadBalancer.Migrations
{
    public partial class ChangingIpToHost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ip",
                table: "Instances",
                newName: "host");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "host",
                table: "Instances",
                newName: "ip");
        }
    }
}
