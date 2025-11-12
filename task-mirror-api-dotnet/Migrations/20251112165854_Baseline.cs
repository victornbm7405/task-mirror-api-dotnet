using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskmirrorapidotnet.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BASELINE: intencionalmente vazio.
            // Não criar/alterar nada, apenas marcar o estado atual como aplicado.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // BASELINE: intencionalmente vazio.
            // Não desfazer nada.
        }
    }
}
