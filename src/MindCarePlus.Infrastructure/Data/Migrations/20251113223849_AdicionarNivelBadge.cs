using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCarePlus.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNivelBadge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Nivel",
                table: "BadgesAtribuidas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nivel",
                table: "BadgesAtribuidas");
        }
    }
}
