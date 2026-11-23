using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bison.Razor.Migrations
{
    /// <inheritdoc />
    public partial class RenameTaxonProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanishVernacularName",
                table: "Taxons");

            migrationBuilder.RenameColumn(
                name: "DwcTaxonId",
                table: "Taxons",
                newName: "dwc_TaxonID");

            migrationBuilder.AddColumn<string>(
                name: "VernacularName",
                table: "Taxons",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VernacularName",
                table: "Taxons");

            migrationBuilder.RenameColumn(
                name: "dwc_TaxonID",
                table: "Taxons",
                newName: "DwcTaxonId");

            migrationBuilder.AddColumn<string>(
                name: "DanishVernacularName",
                table: "Taxons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
