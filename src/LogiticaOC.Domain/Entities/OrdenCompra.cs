namespace LogiticaOC.Domain.Entities;

public class OrdenCompra
{
    public int OcId { get; set; }

    /// <summary>Número único de OC — bloqueado para edición después de creación.</summary>
    public string NumeroOC { get; set; } = string.Empty;

    public int EntidadId { get; set; }
    public Entidad Entidad { get; set; } = null!;

    public int? ResponsableId { get; set; }
    public Responsable? Responsable { get; set; }

    public int EstadoOcId { get; set; }
    public EstadoOC EstadoOC { get; set; } = null!;

    /// <summary>Fecha en que llegó la OC al correo.</summary>
    public DateOnly FechaLlegadaCorreo { get; set; }

    /// <summary>Fecha límite de entrega al cliente.</summary>
    public DateOnly FechaSolicitada { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateOnly? FechaDespacho { get; set; }
    public DateOnly? FechaCierre { get; set; }

    /// <summary>Días totales acordados para la entrega (base para cálculo de alerta 20%).</summary>
    public int? TotalDiasEstablecidos { get; set; }

    public string? LugarEntrega { get; set; }
    public string? Departamento { get; set; }

    /// <summary>Responsable de transporte, nombre de agencia, o "ATENCIÓN DIRECTA".</summary>
    public string? Movilidad { get; set; }

    public string? Observaciones { get; set; }

    /// <summary>PDF adjunto almacenado como binario en BD.</summary>
    public byte[]? PDFAdjunto { get; set; }
    public string? PDFNombreArchivo { get; set; }

    // Navegación
    public ICollection<OrdenCompraDetalle> Detalles { get; set; } = new List<OrdenCompraDetalle>();
    public ICollection<Guia> Guias { get; set; } = new List<Guia>();
    public ICollection<HistorialOC> Historial { get; set; } = new List<HistorialOC>();
}
