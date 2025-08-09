using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class ContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "RemoteFile",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "RemoteFile");
        }
    }
}
