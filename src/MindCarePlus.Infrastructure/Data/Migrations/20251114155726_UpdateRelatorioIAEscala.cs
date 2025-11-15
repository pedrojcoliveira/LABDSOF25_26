using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCarePlus.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelatorioIAEscala : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MediaHumor",
                table: "RelatoriosIA",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "MediaEnergia",
                table: "RelatoriosIA",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MediaStress",
                table: "RelatoriosIA",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TendenciasEnergia",
                table: "RelatoriosIA",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TendenciasStress",
                table: "RelatoriosIA",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaEnergia",
                table: "RelatoriosIA");

            migrationBuilder.DropColumn(
                name: "MediaStress",
                table: "RelatoriosIA");

            migrationBuilder.DropColumn(
                name: "TendenciasEnergia",
                table: "RelatoriosIA");

            migrationBuilder.DropColumn(
                name: "TendenciasStress",
                table: "RelatoriosIA");

            migrationBuilder.AlterColumn<int>(
                name: "MediaHumor",
                table: "RelatoriosIA",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
