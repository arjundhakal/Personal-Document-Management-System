using Microsoft.EntityFrameworkCore.Migrations;

namespace PDMS.Infrastructure.Persistence.Migrations
{
    public partial class AuthenticationIdentifierOnUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Firstname",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationIdentifier",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationSource",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationIdentifier",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthenticationSource",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "Firstname");
        }
    }
}
