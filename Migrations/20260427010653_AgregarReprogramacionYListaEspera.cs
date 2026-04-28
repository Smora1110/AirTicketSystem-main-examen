using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirTicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class AgregarReprogramacionYListaEspera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historial_reprogramacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reserva_id = table.Column<int>(type: "int", nullable: false),
                    vuelo_anterior_id = table.Column<int>(type: "int", nullable: false),
                    vuelo_nuevo_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    motivo = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usuario_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_reprogramacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_historial_reprogramacion_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_historial_reprogramacion_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_historial_reprogramacion_vuelos_vuelo_anterior_id",
                        column: x => x.vuelo_anterior_id,
                        principalTable: "vuelos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_historial_reprogramacion_vuelos_vuelo_nuevo_id",
                        column: x => x.vuelo_nuevo_id,
                        principalTable: "vuelos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lista_espera",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reserva_id = table.Column<int>(type: "int", nullable: false),
                    vuelo_id = table.Column<int>(type: "int", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    prioridad = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, defaultValue: "PENDIENTE")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lista_espera", x => x.id);
                    table.CheckConstraint("chk_estado_lista_espera", "estado IN ('PENDIENTE','PROMOVIDO','CANCELADO')");
                    table.ForeignKey(
                        name: "FK_lista_espera_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lista_espera_vuelos_vuelo_id",
                        column: x => x.vuelo_id,
                        principalTable: "vuelos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_historial_reprogramacion_reserva_id",
                table: "historial_reprogramacion",
                column: "reserva_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_reprogramacion_usuario_id",
                table: "historial_reprogramacion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_reprogramacion_vuelo_anterior_id",
                table: "historial_reprogramacion",
                column: "vuelo_anterior_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_reprogramacion_vuelo_nuevo_id",
                table: "historial_reprogramacion",
                column: "vuelo_nuevo_id");

            migrationBuilder.CreateIndex(
                name: "IX_lista_espera_reserva_id_vuelo_id",
                table: "lista_espera",
                columns: new[] { "reserva_id", "vuelo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lista_espera_vuelo_id",
                table: "lista_espera",
                column: "vuelo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historial_reprogramacion");

            migrationBuilder.DropTable(
                name: "lista_espera");
        }
    }
}
