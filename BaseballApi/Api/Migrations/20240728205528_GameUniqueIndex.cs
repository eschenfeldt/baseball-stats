using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class GameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Games_ExternalId",
                table: "Games",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" != '00000000-0000-0000-0000-000000000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_ExternalId",
                table: "Games");
        }
    }
}
