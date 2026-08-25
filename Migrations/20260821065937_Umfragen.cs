using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intranet2.Migrations
{
    /// <inheritdoc />
    public partial class Umfragen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Umfragen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Frage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartetAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndetAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IstAktiv = table.Column<bool>(type: "bit", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErstelltVon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Umfragen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UmfrageOptionen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UmfrageId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sortierung = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmfrageOptionen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UmfrageOptionen_Umfragen_UmfrageId",
                        column: x => x.UmfrageId,
                        principalTable: "Umfragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UmfrageStimmen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UmfrageId = table.Column<int>(type: "int", nullable: false),
                    UmfrageOptionId = table.Column<int>(type: "int", nullable: false),
                    WindowsBenutzername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AbgestimmtAm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmfrageStimmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UmfrageStimmen_UmfrageOptionen_UmfrageOptionId",
                        column: x => x.UmfrageOptionId,
                        principalTable: "UmfrageOptionen",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UmfrageStimmen_Umfragen_UmfrageId",
                        column: x => x.UmfrageId,
                        principalTable: "Umfragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UmfrageOptionen_UmfrageId",
                table: "UmfrageOptionen",
                column: "UmfrageId");

            migrationBuilder.CreateIndex(
                name: "IX_UmfrageStimmen_UmfrageId_WindowsBenutzername",
                table: "UmfrageStimmen",
                columns: new[] { "UmfrageId", "WindowsBenutzername" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UmfrageStimmen_UmfrageOptionId",
                table: "UmfrageStimmen",
                column: "UmfrageOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UmfrageStimmen");

            migrationBuilder.DropTable(
                name: "UmfrageOptionen");

            migrationBuilder.DropTable(
                name: "Umfragen");
        }
    }
}
