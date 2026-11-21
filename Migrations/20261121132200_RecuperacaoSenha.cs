using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaControleOperacionalApi.Migrations
{
    /// <inheritdoc />
    public partial class RecuperacaoSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecuperacaoExpiraEm",
                table: "Usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecuperacaoTokenHash",
                table: "Usuarios",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecuperacaoExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RecuperacaoTokenHash",
                table: "Usuarios");
        }
    }
}
