using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intranet2.Migrations
{
    /// <inheritdoc />
    public partial class NewsKurzmeldungen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IstKurzmeldung",
                table: "NewsBeitraege",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IstKurzmeldung",
                table: "NewsBeitraege");
        }
    }
}
