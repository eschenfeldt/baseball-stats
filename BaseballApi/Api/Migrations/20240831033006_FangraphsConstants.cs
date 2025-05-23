using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class FangraphsConstants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Constants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    WOBA = table.Column<decimal>(type: "numeric", nullable: false),
                    WOBAScale = table.Column<decimal>(type: "numeric", nullable: false),
                    WBB = table.Column<decimal>(type: "numeric", nullable: false),
                    WHBP = table.Column<decimal>(type: "numeric", nullable: false),
                    W1B = table.Column<decimal>(type: "numeric", nullable: false),
                    W2B = table.Column<decimal>(type: "numeric", nullable: false),
                    W3B = table.Column<decimal>(type: "numeric", nullable: false),
                    WHR = table.Column<decimal>(type: "numeric", nullable: false),
                    RunSB = table.Column<decimal>(type: "numeric", nullable: false),
                    RunCS = table.Column<decimal>(type: "numeric", nullable: false),
                    RPA = table.Column<decimal>(type: "numeric", nullable: false),
                    RW = table.Column<decimal>(type: "numeric", nullable: false),
                    CFIP = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Constants_Year",
                table: "Constants",
                column: "Year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Constants");
        }
    }
}
