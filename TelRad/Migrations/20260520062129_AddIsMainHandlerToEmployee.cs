using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelRad.Migrations
{
    /// <inheritdoc />
    public partial class AddIsMainHandlerToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "NearestTelrad",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsMainHandler",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMainHandler",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "NearestTelrad",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubDepartment",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
