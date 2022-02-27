using Microsoft.EntityFrameworkCore.Migrations;

namespace PDMS.Infrastructure.Persistence.Migrations
{
    public partial class IsAccountActiveOnUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccountActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccountActive",
                table: "Users");
        }
    }
}
