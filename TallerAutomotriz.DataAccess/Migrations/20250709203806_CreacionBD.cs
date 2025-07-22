using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TallerAutomotriz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreacionBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroParte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadDisponible = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuestos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesRepuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRepuesto = table.Column<int>(type: "int", nullable: false),
                    IdSolicitante = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioEntrega = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesRepuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesRepuesto_Repuestos_IdRepuesto",
                        column: x => x.IdRepuesto,
                        principalTable: "Repuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesRepuesto_Usuarios_IdSolicitante",
                        column: x => x.IdSolicitante,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesRepuesto_Usuarios_IdUsuarioEntrega",
                        column: x => x.IdUsuarioEntrega,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_IdRepuesto",
                table: "SolicitudesRepuesto",
                column: "IdRepuesto");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_IdSolicitante",
                table: "SolicitudesRepuesto",
                column: "IdSolicitante");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_IdUsuarioEntrega",
                table: "SolicitudesRepuesto",
                column: "IdUsuarioEntrega");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesRepuesto");

            migrationBuilder.DropTable(
                name: "Repuestos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
