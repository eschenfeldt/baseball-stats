using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class BoxScoreFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GameId",
                table: "BoxScores",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoxScores_Games_GameId",
                table: "BoxScores",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoxScores_Games_GameId",
                table: "BoxScores");

            migrationBuilder.DropIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "BoxScores");
        }
    }
}
