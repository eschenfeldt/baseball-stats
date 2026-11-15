using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class BoxScoreOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppearanceOrder",
                table: "Pitchers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppearanceOrder",
                table: "Batters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineupPosition",
                table: "Batters",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppearanceOrder",
                table: "Pitchers");

            migrationBuilder.DropColumn(
                name: "AppearanceOrder",
                table: "Batters");

            migrationBuilder.DropColumn(
                name: "LineupPosition",
                table: "Batters");
        }
    }
}
