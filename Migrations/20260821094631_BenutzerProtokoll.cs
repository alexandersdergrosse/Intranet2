using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intranet2.Migrations
{
    /// <inheritdoc />
    public partial class BenutzerProtokoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenutzerProtokolle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BenutzerId = table.Column<int>(type: "int", nullable: true),
                    BenutzerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    WindowsBenutzername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Aktion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Feld = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AlterWert = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NeuerWert = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AusgefuehrtVon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Zeitpunkt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenutzerProtokolle", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenutzerProtokolle");
        }
    }
}
