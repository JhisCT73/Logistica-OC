namespace LogiticaOC.Infrastructure.Services;

/// <summary>
/// Contrato para generación de documentos Excel y PDF.
/// Implementado en Infrastructure — nunca referenciado directamente desde Domain.
/// </summary>
public interface IExportService
{
    /// <summary>Genera el Excel de hoja de ruta agrupado por responsable.</summary>
    Task<byte[]> GenerarHojaRutaExcelAsync(IEnumerable<HojaRutaItem> items);

    /// <summary>Genera el PDF de detalle de una OC.</summary>
    Task<byte[]> GenerarOcPdfAsync(int ocId);
}

public record HojaRutaItem(
    string Responsable,
    string NumeroOC,
    string Entidad,
    string Destino,
    string? Agencia
);
