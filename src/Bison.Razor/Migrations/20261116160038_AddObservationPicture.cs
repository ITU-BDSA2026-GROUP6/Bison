using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bison.Razor.Migrations
{
    /// <inheritdoc />
    public partial class AddObservationPicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PictureId",
                table: "Observations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Picture",
                columns: table => new
                {
                    pictureId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Picture", x => x.pictureId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Observations_PictureId",
                table: "Observations",
                column: "PictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Picture_PictureId",
                table: "Observations",
                column: "PictureId",
                principalTable: "Picture",
                principalColumn: "pictureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Picture_PictureId",
                table: "Observations");

            migrationBuilder.DropTable(
                name: "Picture");

            migrationBuilder.DropIndex(
                name: "IX_Observations_PictureId",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "PictureId",
                table: "Observations");
        }
    }
}
