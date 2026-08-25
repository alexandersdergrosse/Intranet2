using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intranet2.Migrations
{
    /// <inheritdoc />
    public partial class WindowsBenutzer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Benutzer");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Benutzer");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Benutzer",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<bool>(
                name: "IstAktiv",
                table: "Benutzer",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Rolle",
                table: "Benutzer",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Benutzer");

            migrationBuilder.AddColumn<string>(
                name: "WindowsBenutzername",
                table: "Benutzer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Benutzer_WindowsBenutzername",
                table: "Benutzer",
                column: "WindowsBenutzername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Benutzer_WindowsBenutzername",
                table: "Benutzer");

            migrationBuilder.DropColumn(
                name: "IstAktiv",
                table: "Benutzer");

            migrationBuilder.DropColumn(
                name: "Rolle",
                table: "Benutzer");

            migrationBuilder.DropColumn(
                name: "WindowsBenutzername",
                table: "Benutzer");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Benutzer",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Benutzer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Benutzer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
