namespace LogiticaOC.Application.DTOs;

// ── Búsqueda avanzada ────────────────────────────────────────────────
public record BuscarOcQuery(
    string? NumeroOC,
    int? EntidadId,
    int? ResponsableId,
    int? EstadoOcId,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta
);

// ── Creación de OC ───────────────────────────────────────────────────
public record CrearOcDto(
    string NumeroOC,
    string RazonSocialEntidad,   // Se busca o crea en Entidades
    int? ResponsableId,
    int EstadoOcId,
    DateOnly FechaLlegadaCorreo,
    DateOnly FechaSolicitada,
    int? TotalDiasEstablecidos,
    string? LugarEntrega,
    string? Departamento,
    string? Movilidad,
    string? Observaciones,
    List<DetalleDto> Detalles
);

// ── Edición de OC (sin NumeroOC) ─────────────────────────────────────
public record EditarOcDto(
    int OcId,
    int? ResponsableId,
    int EstadoOcId,
    DateOnly FechaLlegadaCorreo,
    DateOnly FechaSolicitada,
    DateOnly? FechaDespacho,
    DateOnly? FechaCierre,
    int? TotalDiasEstablecidos,
    string? LugarEntrega,
    string? Departamento,
    string? Movilidad,
    string? Observaciones,
    List<DetalleDto> Detalles
);

// ── Producto / línea ─────────────────────────────────────────────────
public record DetalleDto(
    int DetalleId,           // 0 = nuevo
    string? CodigoProducto,
    string Descripcion,
    decimal Cantidad,
    int EstadoProductoId,
    string? MotivoEntregaParcial
);

// ── Guía ─────────────────────────────────────────────────────────────
public record GuiaDto(
    int GuiaId,              // 0 = nueva
    string NumeroGuia,
    DateOnly FechaGuia,
    string? Responsable
);

// ── Dashboard ────────────────────────────────────────────────────────
public record DashboardItemDto(
    int OcId,
    string NumeroOC,
    string RazonSocial,
    string? Responsable,
    string EstadoOC,
    string ColorEstado,
    DateOnly FechaSolicitada,
    int? TotalDiasEstablecidos,
    int DiasRestantes,
    string AlertaNivel,    // verde | amarillo | rojo | gris
    DateOnly FechaLlegadaCorreo,
    string? LugarEntrega,
    string? Departamento,
    string? Movilidad
);

// ── Hoja de Ruta ─────────────────────────────────────────────────────
public record HojaRutaItemDto(
    string Responsable,
    string NumeroOC,
    string Entidad,
    string? Destino,
    string? Agencia
);

// ── Métricas Dashboard ────────────────────────────────────────────────
public record ClienteMetricasDto(
    string RazonSocial,
    int TotalOcs,
    int Pendientes
);
