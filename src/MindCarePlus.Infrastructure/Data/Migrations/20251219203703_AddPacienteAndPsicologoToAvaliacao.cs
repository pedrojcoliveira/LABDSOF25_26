using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCarePlus.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPacienteAndPsicologoToAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PacienteId",
                table: "Avaliacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PsicologoId",
                table: "Avaliacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_PacienteId",
                table: "Avaliacoes",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_PsicologoId",
                table: "Avaliacoes",
                column: "PsicologoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Utilizador_PacienteId",
                table: "Avaliacoes",
                column: "PacienteId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Utilizador_PsicologoId",
                table: "Avaliacoes",
                column: "PsicologoId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Utilizador_PacienteId",
                table: "Avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Utilizador_PsicologoId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_PacienteId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_PsicologoId",
                table: "Avaliacoes");

            migrationBuilder.DropColumn(
                name: "PacienteId",
                table: "Avaliacoes");

            migrationBuilder.DropColumn(
                name: "PsicologoId",
                table: "Avaliacoes");
        }
    }
}
