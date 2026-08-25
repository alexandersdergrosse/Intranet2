using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intranet2.Migrations
{
    /// <inheritdoc />
    public partial class Marktplatz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LetzterMarktplatzBesuch",
                table: "Benutzer",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarktplatzBeitraege",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kategorie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Preis = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BildPfad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BenutzerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarktplatzBeitraege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarktplatzBeitraege_Benutzer_BenutzerId",
                        column: x => x.BenutzerId,
                        principalTable: "Benutzer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarktplatzBeitraege_BenutzerId",
                table: "MarktplatzBeitraege",
                column: "BenutzerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarktplatzBeitraege");

            migrationBuilder.DropColumn(
                name: "LetzterMarktplatzBesuch",
                table: "Benutzer");
        }
    }
}
