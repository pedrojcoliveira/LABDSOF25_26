using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCarePlus.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    ImagemUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizador",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Idade = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    dataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoUtilizador = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Objectivo = table.Column<string>(type: "text", nullable: true),
                    NivelGamificacao = table.Column<int>(type: "integer", nullable: true),
                    Pontos = table.Column<int>(type: "integer", nullable: true),
                    PsicologoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NumeroCedulaProfissional = table.Column<string>(type: "text", nullable: true),
                    Avaliacao = table.Column<float>(type: "real", nullable: true),
                    Especialidade = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilizador_Utilizador_PsicologoId",
                        column: x => x.PsicologoId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Desafios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    PontosRecompensa = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    BadgeRecompensaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Desafios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Desafios_Badges_BadgeRecompensaId",
                        column: x => x.BadgeRecompensaId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BadgesAtribuidas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilizadorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataAtribuicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgesAtribuidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BadgesAtribuidas_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BadgesAtribuidas_Utilizador_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RemetenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinatarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Counteudo = table.Column<string>(type: "text", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false),
                    DataLeitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_Utilizador_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mensagens_Utilizador_RemetenteId",
                        column: x => x.RemetenteId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistosDiarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    NivelStress = table.Column<int>(type: "integer", nullable: false),
                    NivelHumor = table.Column<int>(type: "integer", nullable: false),
                    NivelEnergia = table.Column<int>(type: "integer", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistosDiarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistosDiarios_Utilizador_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatoriosIA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoRelatorio = table.Column<string>(type: "text", nullable: false),
                    ConteudoGeradoIA = table.Column<string>(type: "text", nullable: false),
                    NivelAlerta = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatoriosIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatoriosIA_Utilizador_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PsicologoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataMarcada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duracao = table.Column<TimeSpan>(type: "interval", nullable: false),
                    LinkSessao = table.Column<string>(type: "text", nullable: false),
                    NotasPsicologo = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessoes_Utilizador_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessoes_Utilizador_PsicologoId",
                        column: x => x.PsicologoId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DesafiosAtribuidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DesafioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilizadorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataAtribuicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesafiosAtribuidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesafiosAtribuidos_Desafios_DesafioId",
                        column: x => x.DesafioId,
                        principalTable: "Desafios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesafiosAtribuidos_Utilizador_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pontuacao = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "text", nullable: false),
                    DataAvaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Sessoes_SessaoId",
                        column: x => x.SessaoId,
                        principalTable: "Sessoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_SessaoId",
                table: "Avaliacoes",
                column: "SessaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BadgesAtribuidas_BadgeId",
                table: "BadgesAtribuidas",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgesAtribuidas_UtilizadorId",
                table: "BadgesAtribuidas",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Desafios_BadgeRecompensaId",
                table: "Desafios",
                column: "BadgeRecompensaId");

            migrationBuilder.CreateIndex(
                name: "IX_DesafiosAtribuidos_DesafioId",
                table: "DesafiosAtribuidos",
                column: "DesafioId");

            migrationBuilder.CreateIndex(
                name: "IX_DesafiosAtribuidos_UtilizadorId",
                table: "DesafiosAtribuidos",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_DestinatarioId",
                table: "Mensagens",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistosDiarios_PacienteId_DataRegisto",
                table: "RegistosDiarios",
                columns: new[] { "PacienteId", "DataRegisto" });

            migrationBuilder.CreateIndex(
                name: "IX_RelatoriosIA_PacienteId",
                table: "RelatoriosIA",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_PacienteId",
                table: "Sessoes",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_PsicologoId",
                table: "Sessoes",
                column: "PsicologoId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_Email",
                table: "Utilizador",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_PsicologoId",
                table: "Utilizador",
                column: "PsicologoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "BadgesAtribuidas");

            migrationBuilder.DropTable(
                name: "DesafiosAtribuidos");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "RegistosDiarios");

            migrationBuilder.DropTable(
                name: "RelatoriosIA");

            migrationBuilder.DropTable(
                name: "Sessoes");

            migrationBuilder.DropTable(
                name: "Desafios");

            migrationBuilder.DropTable(
                name: "Utilizador");

            migrationBuilder.DropTable(
                name: "Badges");
        }
    }
}
