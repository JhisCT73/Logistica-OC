using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiticaOC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedEstados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 1,
                column: "ColorHex",
                value: "#4f8ef7");

            migrationBuilder.UpdateData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 3,
                columns: new[] { "ColorHex", "Nombre" },
                values: new object[] { "#f59e0b", "Entrega Parcial" });

            migrationBuilder.UpdateData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 4,
                columns: new[] { "ColorHex", "Nombre" },
                values: new object[] { "#22c55e", "Cerrada" });

            migrationBuilder.InsertData(
                table: "EstadosOC",
                columns: new[] { "EstadoOcId", "ColorHex", "Nombre" },
                values: new object[] { 5, "#6c757d", "Cancelada" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 1,
                column: "ColorHex",
                value: "#0d6efd");

            migrationBuilder.UpdateData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 3,
                columns: new[] { "ColorHex", "Nombre" },
                values: new object[] { "#6c757d", "Cerrada" });

            migrationBuilder.UpdateData(
                table: "EstadosOC",
                keyColumn: "EstadoOcId",
                keyValue: 4,
                columns: new[] { "ColorHex", "Nombre" },
                values: new object[] { "#343a40", "Cancelada" });
        }
    }
}
