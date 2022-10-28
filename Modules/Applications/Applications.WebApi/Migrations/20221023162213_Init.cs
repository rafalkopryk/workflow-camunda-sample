using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Applications.WebApi.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditApplication",
                columns: table => new
                {
                    ApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditPeriodInMonths = table.Column<int>(type: "int", nullable: false),
                    CustomerPersonalData_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPersonalData_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPersonalData_Pesel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Declaration_AverageNetMonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplication", x => x.ApplicationId);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    CreditApplicationApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ContractSigningDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Decision = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => new { x.CreditApplicationApplicationId, x.Id });
                    table.ForeignKey(
                        name: "FK_State_CreditApplication_CreditApplicationApplicationId",
                        column: x => x.CreditApplicationApplicationId,
                        principalTable: "CreditApplication",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "CreditApplication");
        }
    }
}
