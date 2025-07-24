using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class MediaImportFileCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FilesDeleted",
                table: "MediaImportInfo",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilesDeleted",
                table: "MediaImportInfo");
        }
    }
}
