using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class ReferencePlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MLBAMId",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReferencePlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    FangraphsId = table.Column<string>(type: "text", nullable: true),
                    BaseballReferenceId = table.Column<string>(type: "text", nullable: true),
                    MLBAMId = table.Column<string>(type: "text", nullable: true),
                    RetrosheetId = table.Column<string>(type: "text", nullable: true),
                    CurrentNumber = table.Column<int>(type: "integer", nullable: true),
                    CurrentTeamId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferencePlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferencePlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReferencePlayers_Teams_CurrentTeamId",
                        column: x => x.CurrentTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferencePlayers_CurrentTeamId",
                table: "ReferencePlayers",
                column: "CurrentTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferencePlayers_PlayerId",
                table: "ReferencePlayers",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferencePlayers");

            migrationBuilder.DropColumn(
                name: "MLBAMId",
                table: "Teams");
        }
    }
}
