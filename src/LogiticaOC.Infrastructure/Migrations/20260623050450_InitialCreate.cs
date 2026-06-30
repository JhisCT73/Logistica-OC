using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogiticaOC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entidades",
                columns: table => new
                {
                    EntidadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entidades", x => x.EntidadId);
                });

            migrationBuilder.CreateTable(
                name: "EstadosOC",
                columns: table => new
                {
                    EstadoOcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosOC", x => x.EstadoOcId);
                });

            migrationBuilder.CreateTable(
                name: "EstadosProducto",
                columns: table => new
                {
                    EstadoProductoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosProducto", x => x.EstadoProductoId);
                });

            migrationBuilder.CreateTable(
                name: "Responsables",
                columns: table => new
                {
                    ResponsableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsables", x => x.ResponsableId);
                });

            migrationBuilder.CreateTable(
                name: "OrdenCompra",
                columns: table => new
                {
                    OcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroOC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: true),
                    EstadoOcId = table.Column<int>(type: "int", nullable: false),
                    FechaLlegadaCorreo = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaSolicitada = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FechaDespacho = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCierre = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalDiasEstablecidos = table.Column<int>(type: "int", nullable: true),
                    LugarEntrega = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Movilidad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PDFAdjunto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PDFNombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenCompra", x => x.OcId);
                    table.ForeignKey(
                        name: "FK_OrdenCompra_Entidades_EntidadId",
                        column: x => x.EntidadId,
                        principalTable: "Entidades",
                        principalColumn: "EntidadId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenCompra_EstadosOC_EstadoOcId",
                        column: x => x.EstadoOcId,
                        principalTable: "EstadosOC",
                        principalColumn: "EstadoOcId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenCompra_Responsables_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Responsables",
                        principalColumn: "ResponsableId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Guias",
                columns: table => new
                {
                    GuiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcId = table.Column<int>(type: "int", nullable: false),
                    NumeroGuia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaGuia = table.Column<DateOnly>(type: "date", nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guias", x => x.GuiaId);
                    table.ForeignKey(
                        name: "FK_Guias_OrdenCompra_OcId",
                        column: x => x.OcId,
                        principalTable: "OrdenCompra",
                        principalColumn: "OcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialOC",
                columns: table => new
                {
                    HistorialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcId = table.Column<int>(type: "int", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsableAnterior = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsableNuevo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialOC", x => x.HistorialId);
                    table.ForeignKey(
                        name: "FK_HistorialOC_OrdenCompra_OcId",
                        column: x => x.OcId,
                        principalTable: "OrdenCompra",
                        principalColumn: "OcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenCompraDetalle",
                columns: table => new
                {
                    DetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcId = table.Column<int>(type: "int", nullable: false),
                    CodigoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstadoProductoId = table.Column<int>(type: "int", nullable: false),
                    MotivoEntregaParcial = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenCompraDetalle", x => x.DetalleId);
                    table.ForeignKey(
                        name: "FK_OrdenCompraDetalle_EstadosProducto_EstadoProductoId",
                        column: x => x.EstadoProductoId,
                        principalTable: "EstadosProducto",
                        principalColumn: "EstadoProductoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenCompraDetalle_OrdenCompra_OcId",
                        column: x => x.OcId,
                        principalTable: "OrdenCompra",
                        principalColumn: "OcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EstadosOC",
                columns: new[] { "EstadoOcId", "ColorHex", "Nombre" },
                values: new object[,]
                {
                    { 1, "#0d6efd", "Pendiente" },
                    { 2, "#6f42c1", "Listo para Despacho" },
                    { 3, "#6c757d", "Cerrada" },
                    { 4, "#343a40", "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "EstadosProducto",
                columns: new[] { "EstadoProductoId", "ColorHex", "Nombre" },
                values: new object[,]
                {
                    { 1, "#28a745", "En Proceso" },
                    { 2, "#fd7e14", "Urgente Compras" },
                    { 3, "#0dcaf0", "Importación" },
                    { 4, "#ffc107", "Entrega Parcial" },
                    { 5, "#6c757d", "Cerrada" },
                    { 6, "#343a40", "Cancelada" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entidades_RazonSocial",
                table: "Entidades",
                column: "RazonSocial");

            migrationBuilder.CreateIndex(
                name: "IX_Guias_OcId",
                table: "Guias",
                column: "OcId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialOC_OcId",
                table: "HistorialOC",
                column: "OcId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_EntidadId",
                table: "OrdenCompra",
                column: "EntidadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_EstadoOcId",
                table: "OrdenCompra",
                column: "EstadoOcId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_FechaRegistro",
                table: "OrdenCompra",
                column: "FechaRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_NumeroOC",
                table: "OrdenCompra",
                column: "NumeroOC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_ResponsableId",
                table: "OrdenCompra",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompraDetalle_EstadoProductoId",
                table: "OrdenCompraDetalle",
                column: "EstadoProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompraDetalle_OcId",
                table: "OrdenCompraDetalle",
                column: "OcId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guias");

            migrationBuilder.DropTable(
                name: "HistorialOC");

            migrationBuilder.DropTable(
                name: "OrdenCompraDetalle");

            migrationBuilder.DropTable(
                name: "EstadosProducto");

            migrationBuilder.DropTable(
                name: "OrdenCompra");

            migrationBuilder.DropTable(
                name: "Entidades");

            migrationBuilder.DropTable(
                name: "EstadosOC");

            migrationBuilder.DropTable(
                name: "Responsables");
        }
    }
}
