using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bison.Razor.Migrations
{
    /// <inheritdoc />
    public partial class LinkAuthorsToIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BisonUserId",
                table: "Authors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Authors_BisonUserId",
                table: "Authors",
                column: "BisonUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_AspNetUsers_BisonUserId",
                table: "Authors",
                column: "BisonUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_AspNetUsers_BisonUserId",
                table: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_Authors_BisonUserId",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "BisonUserId",
                table: "Authors");
        }
    }
}
