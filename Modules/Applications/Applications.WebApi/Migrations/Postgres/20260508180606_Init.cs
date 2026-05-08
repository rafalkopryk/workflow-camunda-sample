using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Applications.WebApi.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditApplication",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreditPeriodInMonths = table.Column<int>(type: "integer", nullable: false),
                    States = table.Column<string>(type: "text", nullable: false),
                    CustomerPersonalData = table.Column<string>(type: "jsonb", nullable: false),
                    Declaration = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplication", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditApplication");
        }
    }
}
