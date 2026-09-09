using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IndicVest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    IdCountry = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ISOCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.IdCountry);
                });

            migrationBuilder.CreateTable(
                name: "MacroIndicators",
                columns: table => new
                {
                    IdMacroIndicator = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    IsHighBetter = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MacroIndicators", x => x.IdMacroIndicator);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRates",
                columns: table => new
                {
                    IdReturnRate = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinReturnRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    MaxReturnRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRates", x => x.IdReturnRate);
                    table.CheckConstraint("CK_ReturnRate_MinLessThanMax", "\"MinReturnRate\" < \"MaxReturnRate\"");
                });

            migrationBuilder.CreateTable(
                name: "Indicators",
                columns: table => new
                {
                    IdIndicator = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCountry = table.Column<int>(type: "integer", nullable: false),
                    IdMacroIndicator = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicators", x => x.IdIndicator);
                    table.ForeignKey(
                        name: "FK_Indicators_Countries_IdCountry",
                        column: x => x.IdCountry,
                        principalTable: "Countries",
                        principalColumn: "IdCountry",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Indicators_MacroIndicators_IdMacroIndicator",
                        column: x => x.IdMacroIndicator,
                        principalTable: "MacroIndicators",
                        principalColumn: "IdMacroIndicator",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ReturnRates",
                columns: new[] { "IdReturnRate", "MaxReturnRate", "MinReturnRate" },
                values: new object[] { 1, 0.15m, 0.02m });

            migrationBuilder.CreateIndex(
                name: "IX_Countries_ISOCode",
                table: "Countries",
                column: "ISOCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Indicators_IdCountry_IdMacroIndicator_Year",
                table: "Indicators",
                columns: new[] { "IdCountry", "IdMacroIndicator", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Indicators_IdMacroIndicator",
                table: "Indicators",
                column: "IdMacroIndicator");

            migrationBuilder.CreateIndex(
                name: "IX_MacroIndicators_Name",
                table: "MacroIndicators",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Indicators");

            migrationBuilder.DropTable(
                name: "ReturnRates");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "MacroIndicators");
        }
    }
}
