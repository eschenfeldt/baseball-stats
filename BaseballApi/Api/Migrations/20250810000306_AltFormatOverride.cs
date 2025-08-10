using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class AltFormatOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlternateFormatOverride",
                table: "RemoteResource",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternateFormatOverride",
                table: "RemoteResource");
        }
    }
}
