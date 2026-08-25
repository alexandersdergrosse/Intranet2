using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intranet2.Migrations
{
    /// <inheritdoc />
    public partial class NewsVerwaltung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsBeitraege",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kurztext = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Inhalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kategorie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KategorieFarbe = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BildPfad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VeroeffentlichtAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IstVeroeffentlicht = table.Column<bool>(type: "bit", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErstelltVon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    GeaendertAm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsBeitraege", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsBeitraege");
        }
    }
}
