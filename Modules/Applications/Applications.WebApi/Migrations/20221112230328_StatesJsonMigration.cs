using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Applications.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class StatesJsonMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.AddColumn<string>(
                name: "States",
                table: "CreditApplication",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "States",
                table: "CreditApplication");

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    CreditApplicationApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractSigningDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
    }
}
