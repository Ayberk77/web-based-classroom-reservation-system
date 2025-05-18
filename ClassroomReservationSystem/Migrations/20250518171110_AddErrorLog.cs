using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Exception",
                table: "ErrorLogs",
                newName: "StackTrace");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryString",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestPath",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "QueryString",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestPath",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "ErrorLogs");

            migrationBuilder.RenameColumn(
                name: "StackTrace",
                table: "ErrorLogs",
                newName: "Exception");
        }
    }
}
