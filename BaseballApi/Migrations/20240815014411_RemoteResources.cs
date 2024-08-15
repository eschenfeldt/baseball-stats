using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoteResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ScorecardId",
                table: "Games",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RemoteResource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetIdentifier = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Favorite = table.Column<bool>(type: "boolean", nullable: true),
                    ResourceType = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemoteResource_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MediaResourcePlayer",
                columns: table => new
                {
                    MediaId = table.Column<long>(type: "bigint", nullable: false),
                    PlayersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaResourcePlayer", x => new { x.MediaId, x.PlayersId });
                    table.ForeignKey(
                        name: "FK_MediaResourcePlayer_Players_PlayersId",
                        column: x => x.PlayersId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaResourcePlayer_RemoteResource_MediaId",
                        column: x => x.MediaId,
                        principalTable: "RemoteResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemoteFile",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResourceId = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    NameModifier = table.Column<string>(type: "text", nullable: true),
                    Extension = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemoteFile_RemoteResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "RemoteResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_ScorecardId",
                table: "Games",
                column: "ScorecardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaResourcePlayer_PlayersId",
                table: "MediaResourcePlayer",
                column: "PlayersId");

            migrationBuilder.CreateIndex(
                name: "IX_RemoteFile_ResourceId",
                table: "RemoteFile",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RemoteFile_ResourceId_NameModifier_Extension",
                table: "RemoteFile",
                columns: new[] { "ResourceId", "NameModifier", "Extension" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemoteResource_AssetIdentifier",
                table: "RemoteResource",
                column: "AssetIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemoteResource_GameId",
                table: "RemoteResource",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_RemoteResource_ScorecardId",
                table: "Games",
                column: "ScorecardId",
                principalTable: "RemoteResource",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_RemoteResource_ScorecardId",
                table: "Games");

            migrationBuilder.DropTable(
                name: "MediaResourcePlayer");

            migrationBuilder.DropTable(
                name: "RemoteFile");

            migrationBuilder.DropTable(
                name: "RemoteResource");

            migrationBuilder.DropIndex(
                name: "IX_Games_ScorecardId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ScorecardId",
                table: "Games");
        }
    }
}
