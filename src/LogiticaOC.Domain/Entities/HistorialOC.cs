namespace LogiticaOC.Domain.Entities;

/// <summary>
/// Registro cronológico de cada cambio de estado o responsable en una OC.
/// Se genera automáticamente en el service — nunca manualmente.
/// </summary>
public class HistorialOC
{
    public int HistorialId { get; set; }

    public int OcId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;

    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public string? ResponsableAnterior { get; set; }
    public string? ResponsableNuevo { get; set; }
    public string? Observacion { get; set; }
}
