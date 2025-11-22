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
            migrationBuilder.AddColumn<int>(
                name: "MLBAMId",
                table: "Teams",
                type: "integer",
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
                    MLBAMId = table.Column<int>(type: "integer", nullable: true),
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

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION public.unaccent_immutable(text)
                RETURNS text
                LANGUAGE sql
                IMMUTABLE
                AS $$
                SELECT unaccent($1);
                $$;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Players""
                DROP COLUMN IF EXISTS name_normalized;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Players""
                ADD COLUMN IF NOT EXISTS name_normalized text
                    GENERATED ALWAYS AS (public.unaccent_immutable(lower(""Name""))) STORED;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_players_name_normalized
                ON ""Players"" (name_normalized);
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferencePlayers");

            migrationBuilder.DropColumn(
                name: "MLBAMId",
                table: "Teams");

            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_players_name_normalized;");
            migrationBuilder.DropColumn(
                name: "name_normalized",
                table: "Players"
            );
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.unaccent_immutable(text);");

            migrationBuilder.Sql("DROP EXTENSION IF EXISTS unaccent;");
        }
    }
}
