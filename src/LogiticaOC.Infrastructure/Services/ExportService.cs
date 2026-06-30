using ClosedXML.Excel;
using LogiticaOC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LogiticaOC.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly OcLogisticaDbContext _db;

    public ExportService(OcLogisticaDbContext db) => _db = db;

    // ── Hoja de Ruta Excel ───────────────────────────────────────────
    public Task<byte[]> GenerarHojaRutaExcelAsync(IEnumerable<HojaRutaItem> items)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Hoja de Ruta");

        // Encabezados
        var headers = new[] { "Responsable", "N° OC", "Entidad", "Destino", "Agencia" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Datos agrupados por responsable
        int row = 2;
        string? ultimoResponsable = null;
        foreach (var item in items.OrderBy(x => x.Responsable))
        {
            if (item.Responsable != ultimoResponsable)
            {
                // Fila de agrupación
                var groupCell = ws.Cell(row, 1);
                groupCell.Value = item.Responsable;
                ws.Range(row, 1, row, 5).Merge();
                groupCell.Style.Font.Bold = true;
                groupCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f0fe");
                ultimoResponsable = item.Responsable;
                row++;
            }

            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = item.NumeroOC;
            ws.Cell(row, 3).Value = item.Entidad;
            ws.Cell(row, 4).Value = item.Destino ?? "";
            ws.Cell(row, 5).Value = item.Agencia ?? "";
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.Range(1, 1, row - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    // ── PDF de OC ────────────────────────────────────────────────────
    public async Task<byte[]> GenerarOcPdfAsync(int ocId)
    {
        var oc = await _db.OrdenesCompra
            .Include(x => x.Entidad)
            .Include(x => x.Responsable)
            .Include(x => x.EstadoOC)
            .Include(x => x.Detalles).ThenInclude(d => d.EstadoProducto)
            .Include(x => x.Guias)
            .FirstOrDefaultAsync(x => x.OcId == ocId)
            ?? throw new KeyNotFoundException($"OC {ocId} no encontrada");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text($"Orden de Compra: {oc.NumeroOC}")
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken3);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Entidad: {oc.Entidad.RazonSocial}");
                    col.Item().Text($"Responsable: {oc.Responsable?.Nombre ?? "-"}");
                    col.Item().Text($"Estado: {oc.EstadoOC.Nombre}");
                    col.Item().Text($"Fecha solicitada: {oc.FechaSolicitada}");
                    col.Item().Text($"Lugar entrega: {oc.LugarEntrega ?? "-"}");
                    col.Item().PaddingVertical(8).LineHorizontal(1);

                    col.Item().Text("Productos").SemiBold().FontSize(12);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80);
                            c.RelativeColumn();
                            c.ConstantColumn(60);
                            c.ConstantColumn(100);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Text("Código").SemiBold();
                            h.Cell().Text("Descripción").SemiBold();
                            h.Cell().Text("Cantidad").SemiBold();
                            h.Cell().Text("Estado").SemiBold();
                        });

                        foreach (var d in oc.Detalles)
                        {
                            table.Cell().Text(d.CodigoProducto ?? "-");
                            table.Cell().Text(d.Descripcion);
                            table.Cell().Text(d.Cantidad.ToString("N2"));
                            table.Cell().Text(d.EstadoProducto.Nombre);
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
            });
        }).GeneratePdf();

        return pdfBytes;
    }
}
