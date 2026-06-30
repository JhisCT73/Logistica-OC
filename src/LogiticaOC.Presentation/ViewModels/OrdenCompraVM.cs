using System.ComponentModel.DataAnnotations;
using LogiticaOC.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LogiticaOC.Presentation.ViewModels;

public class OrdenCompraFormVM
{
    public int OcId { get; set; }

    [Display(Name = "Número de OC")]
    [Required(ErrorMessage = "El número de OC es obligatorio")]
    public string NumeroOC { get; set; } = string.Empty;

    public bool EsEdicion => OcId > 0;

    [Display(Name = "Entidad / Razón Social")]
    [Required(ErrorMessage = "La entidad es obligatoria")]
    public string RazonSocialEntidad { get; set; } = string.Empty;

    [Display(Name = "Responsable")]
    public int? ResponsableId { get; set; }

    [Display(Name = "Estado OC")]
    [Required]
    public int EstadoOcId { get; set; }

    [Display(Name = "Fecha llegada al correo")]
    [Required]
    public DateOnly FechaLlegadaCorreo { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Fecha solicitada (límite)")]
    [Required]
    public DateOnly FechaSolicitada { get; set; }

    [Display(Name = "Fecha despacho")]
    public DateOnly? FechaDespacho { get; set; }

    [Display(Name = "Fecha cierre")]
    public DateOnly? FechaCierre { get; set; }

    [Display(Name = "Días establecidos")]
    public int? TotalDiasEstablecidos { get; set; }

    [Display(Name = "Lugar de entrega")]
    public string? LugarEntrega { get; set; }

    [Display(Name = "Departamento")]
    public string? Departamento { get; set; }

    [Display(Name = "Movilidad / Agencia")]
    public string? Movilidad { get; set; }

    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    // Detalles de productos
    public List<DetalleVM> Detalles { get; set; } = new();

    // Guías (solo en edición)
    public List<GuiaVM> Guias { get; set; } = new();

    // Historial (solo en detalle)
    public List<HistorialOC> Historial { get; set; } = new();

    // Selectlists
    public SelectList? Responsables { get; set; }
    public SelectList? EstadosOC { get; set; }
    public SelectList? EstadosProducto { get; set; }

    // Nombre del PDF adjunto (para mostrar si existe)
    public string? PDFNombreArchivo { get; set; }
}

public class DetalleVM
{
    public int DetalleId { get; set; }

    [Display(Name = "Código")]
    public string? CodigoProducto { get; set; }

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "La descripción es obligatoria")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Cantidad")]
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }

    [Display(Name = "Estado")]
    public int EstadoProductoId { get; set; }

    [Display(Name = "Motivo Entrega Parcial")]
    public string? MotivoEntregaParcial { get; set; }

    public string EstadoNombre { get; set; } = string.Empty;
    public string EstadoColor { get; set; } = "#6c757d";
}

public class GuiaVM
{
    public int GuiaId { get; set; }

    [Display(Name = "N° Guía")]
    [Required]
    public string NumeroGuia { get; set; } = string.Empty;

    [Display(Name = "Fecha")]
    [Required]
    public DateOnly FechaGuia { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Responsable")]
    public string? Responsable { get; set; }
}

// ── Filtros dashboard/búsqueda ────────────────────────────────────────
public class BusquedaVM
{
    public string? NumeroOC { get; set; }
    public int? EntidadId { get; set; }
    public int? ResponsableId { get; set; }
    public int? EstadoOcId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }

    public SelectList? Responsables { get; set; }
    public SelectList? EstadosOC { get; set; }
    public SelectList? Entidades { get; set; }
}
