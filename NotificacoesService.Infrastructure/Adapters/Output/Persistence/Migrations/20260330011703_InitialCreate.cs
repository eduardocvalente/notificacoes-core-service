using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificacoesService.Infrastructure.Adapters.Output.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    destinatario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assunto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    corpo = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tentativas_envio = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    criada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    enviada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_falha = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacoes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_destinatario_id",
                table: "notificacoes",
                column: "destinatario_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_status",
                table: "notificacoes",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificacoes");
        }
    }
}
