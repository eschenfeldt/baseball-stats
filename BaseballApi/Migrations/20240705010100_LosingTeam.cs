using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class LosingTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LosingTeamId",
                table: "Games",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_LosingTeamId",
                table: "Games",
                column: "LosingTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_LosingTeamId",
                table: "Games",
                column: "LosingTeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_LosingTeamId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_LosingTeamId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "LosingTeamId",
                table: "Games");
        }
    }
}
