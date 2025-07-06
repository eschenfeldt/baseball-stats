using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class MediaImportTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaImportTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaImportTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaImportTasks_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MediaImportInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseName = table.Column<string>(type: "text", nullable: false),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    PhotoFilePath = table.Column<string>(type: "text", nullable: true),
                    PhotoFileName = table.Column<string>(type: "text", nullable: true),
                    VideoFilePath = table.Column<string>(type: "text", nullable: true),
                    VideoFileName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MediaImportTaskId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaImportInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaImportInfo_MediaImportTasks_MediaImportTaskId",
                        column: x => x.MediaImportTaskId,
                        principalTable: "MediaImportTasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaImportInfo_MediaImportTaskId",
                table: "MediaImportInfo",
                column: "MediaImportTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaImportTasks_GameId",
                table: "MediaImportTasks",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaImportInfo");

            migrationBuilder.DropTable(
                name: "MediaImportTasks");
        }
    }
}
