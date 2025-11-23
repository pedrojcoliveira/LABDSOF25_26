using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCarePlus.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatoriosIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TipoRelatorio",
                table: "RelatoriosIA",
                newName: "TendenciasHumor");

            migrationBuilder.RenameColumn(
                name: "NivelAlerta",
                table: "RelatoriosIA",
                newName: "RelatorioCompleto");

            migrationBuilder.RenameColumn(
                name: "ConteudoGeradoIA",
                table: "RelatoriosIA",
                newName: "Recomendacoes");

            migrationBuilder.AddColumn<bool>(
                name: "Lido",
                table: "RelatoriosIA",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MediaHumor",
                table: "RelatoriosIA",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodoFim",
                table: "RelatoriosIA",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodoInicio",
                table: "RelatoriosIA",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lido",
                table: "RelatoriosIA");

            migrationBuilder.DropColumn(
                name: "MediaHumor",
                table: "RelatoriosIA");

            migrationBuilder.DropColumn(
                name: "PeriodoFim",
                table: "RelatoriosIA");

            migrationBuilder.DropColumn(
                name: "PeriodoInicio",
                table: "RelatoriosIA");

            migrationBuilder.RenameColumn(
                name: "TendenciasHumor",
                table: "RelatoriosIA",
                newName: "TipoRelatorio");

            migrationBuilder.RenameColumn(
                name: "RelatorioCompleto",
                table: "RelatoriosIA",
                newName: "NivelAlerta");

            migrationBuilder.RenameColumn(
                name: "Recomendacoes",
                table: "RelatoriosIA",
                newName: "ConteudoGeradoIA");
        }
    }
}
