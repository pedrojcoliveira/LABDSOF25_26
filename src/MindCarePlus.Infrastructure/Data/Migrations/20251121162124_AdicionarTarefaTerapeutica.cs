using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCarePlus.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTarefaTerapeutica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTarefaTerapeutica",
                table: "DesafiosAtribuidos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Prazo",
                table: "DesafiosAtribuidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PsicologoId",
                table: "DesafiosAtribuidos",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTarefaTerapeutica",
                table: "DesafiosAtribuidos");

            migrationBuilder.DropColumn(
                name: "Prazo",
                table: "DesafiosAtribuidos");

            migrationBuilder.DropColumn(
                name: "PsicologoId",
                table: "DesafiosAtribuidos");
        }
    }
}
