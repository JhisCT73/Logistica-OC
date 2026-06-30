using LogiticaOC.Application.DTOs;
using LogiticaOC.Domain.Entities;
using LogiticaOC.Domain.Interfaces;
using LogiticaOC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Infrastructure.Services;

public class OrdenCompraService
{
    private readonly IOrdenCompraRepository _repo;
    private readonly IEntidadRepository _entidadRepo;
    private readonly OcLogisticaDbContext _db;
    private readonly IExportService _export;

    public OrdenCompraService(
        IOrdenCompraRepository repo,
        IEntidadRepository entidadRepo,
        OcLogisticaDbContext db,
        IExportService export)
    {
        _repo = repo;
        _entidadRepo = entidadRepo;
        _db = db;
        _export = export;
    }

    // ── CREAR ────────────────────────────────────────────────────────
    public async Task<int> CrearAsync(CrearOcDto dto)
    {
        if (await _repo.ExisteNumeroOCAsync(dto.NumeroOC))
            throw new InvalidOperationException($"Ya existe una OC con el número '{dto.NumeroOC}'.");

        var entidad = await _entidadRepo.ObtenerOCrearAsync(dto.RazonSocialEntidad);

        var oc = new OrdenCompra
        {
            NumeroOC            = dto.NumeroOC.Trim(),
            EntidadId           = entidad.EntidadId,
            ResponsableId       = dto.ResponsableId,
            EstadoOcId          = dto.EstadoOcId,
            FechaLlegadaCorreo  = dto.FechaLlegadaCorreo,
            FechaSolicitada     = dto.FechaSolicitada,
            TotalDiasEstablecidos = dto.TotalDiasEstablecidos,
            LugarEntrega        = dto.LugarEntrega,
            Departamento        = dto.Departamento,
            Movilidad           = dto.Movilidad,
            Observaciones       = dto.Observaciones,
            FechaRegistro       = DateTime.UtcNow,
        };

        // Detalles
        foreach (var d in dto.Detalles)
            oc.Detalles.Add(new OrdenCompraDetalle
            {
                CodigoProducto       = d.CodigoProducto,
                Descripcion          = d.Descripcion,
                Cantidad             = d.Cantidad,
                EstadoProductoId     = d.EstadoProductoId,
                MotivoEntregaParcial = d.MotivoEntregaParcial,
            });

        // Historial inicial
        oc.Historial.Add(new HistorialOC
        {
            EstadoNuevo  = "Creada",
            Observacion  = "OC registrada en el sistema",
            FechaModificacion = DateTime.UtcNow
        });

        await _repo.AddAsync(oc);
        await _repo.SaveChangesAsync();
        return oc.OcId;
    }

    // ── EDITAR ───────────────────────────────────────────────────────
    public async Task EditarAsync(EditarOcDto dto, string? observacion = null)
    {
        var oc = await _repo.GetByIdAsync(dto.OcId, includeDetalles: true)
            ?? throw new KeyNotFoundException($"OC {dto.OcId} no encontrada.");

        // Capturar estado anterior para historial
        var estadoAnteriorNombre = oc.EstadoOC?.Nombre;
        var responsableAnteriorNombre = oc.Responsable?.Nombre;

        oc.ResponsableId          = dto.ResponsableId;
        oc.EstadoOcId             = dto.EstadoOcId;
        oc.FechaLlegadaCorreo     = dto.FechaLlegadaCorreo;
        oc.FechaSolicitada        = dto.FechaSolicitada;
        oc.FechaDespacho          = dto.FechaDespacho;
        oc.FechaCierre            = dto.FechaCierre;
        oc.TotalDiasEstablecidos  = dto.TotalDiasEstablecidos;
        oc.LugarEntrega           = dto.LugarEntrega;
        oc.Departamento           = dto.Departamento;
        oc.Movilidad              = dto.Movilidad;
        oc.Observaciones          = dto.Observaciones;

        // Sincronizar detalles
        SincronizarDetalles(oc, dto.Detalles);

        // Registrar historial solo si cambió algo relevante
        var estadoNuevo     = await _db.EstadosOC.FindAsync(dto.EstadoOcId);
        var responsableNuevo = dto.ResponsableId.HasValue
            ? await _db.Responsables.FindAsync(dto.ResponsableId.Value)
            : null;

        bool huboCambio = estadoAnteriorNombre != estadoNuevo?.Nombre
            || responsableAnteriorNombre != responsableNuevo?.Nombre;

        if (huboCambio || !string.IsNullOrWhiteSpace(observacion))
        {
            oc.Historial.Add(new HistorialOC
            {
                EstadoAnterior       = estadoAnteriorNombre,
                EstadoNuevo          = estadoNuevo?.Nombre,
                ResponsableAnterior  = responsableAnteriorNombre,
                ResponsableNuevo     = responsableNuevo?.Nombre,
                Observacion          = observacion,
                FechaModificacion    = DateTime.UtcNow
            });
        }

        _repo.Update(oc);
        await _repo.SaveChangesAsync();
    }

    // ── CANCELAR (no eliminar) ────────────────────────────────────────
    public async Task CancelarAsync(int ocId, string motivo)
    {
        var oc = await _repo.GetByIdAsync(ocId)
            ?? throw new KeyNotFoundException($"OC {ocId} no encontrada.");

        var estadoCancelada = await _db.EstadosOC.FirstAsync(e => e.Nombre == "Cancelada");
        var estadoAnterior  = oc.EstadoOC?.Nombre;

        oc.EstadoOcId = estadoCancelada.EstadoOcId;

        oc.Historial.Add(new HistorialOC
        {
            EstadoAnterior    = estadoAnterior,
            EstadoNuevo       = "Cancelada",
            Observacion       = motivo,
            FechaModificacion = DateTime.UtcNow
        });

        _repo.Update(oc);
        await _repo.SaveChangesAsync();
    }

    // ── DASHBOARD ────────────────────────────────────────────────────
    public async Task<IEnumerable<DashboardItemDto>> ObtenerDashboardAsync()
    {
        var ocs = await _repo.GetAllAsync();
        return ocs.Select(oc => MapToDashboard(oc)).ToList();
    }

    // ── HOJA DE RUTA ─────────────────────────────────────────────────
    public async Task<byte[]> GenerarHojaRutaAsync()
    {
        var estadosExcluidos = new[] { "Cerrada", "Cancelada", "Listo para Despacho" };
        var ocs = (await _repo.GetAllAsync())
            .Where(oc => !estadosExcluidos.Contains(oc.EstadoOC.Nombre))
            .ToList();

        var items = ocs.Select(oc => new HojaRutaItem(
            Responsable: oc.Responsable?.Nombre ?? "Sin asignar",
            NumeroOC:    oc.NumeroOC,
            Entidad:     oc.Entidad.RazonSocial,
            Destino:     oc.LugarEntrega ?? "",
            Agencia:     oc.Movilidad
        )).ToList();

        // Cambiar estado a "Listo para Despacho"
        var estadoListo = await _db.EstadosOC.FirstAsync(e => e.Nombre == "Listo para Despacho");
        foreach (var oc in ocs)
        {
            var estadoAnterior = oc.EstadoOC.Nombre;
            oc.EstadoOcId = estadoListo.EstadoOcId;
            oc.Historial.Add(new HistorialOC
            {
                EstadoAnterior    = estadoAnterior,
                EstadoNuevo       = "Listo para Despacho",
                Observacion       = "Generación automática de hoja de ruta",
                FechaModificacion = DateTime.UtcNow
            });
            _repo.Update(oc);
        }
        await _repo.SaveChangesAsync();

        return await _export.GenerarHojaRutaExcelAsync(items);
    }

    // ── UPLOAD PDF ───────────────────────────────────────────────────
    public async Task GuardarPdfAsync(int ocId, Stream pdfStream, string nombreArchivo)
    {
        var oc = await _repo.GetByIdAsync(ocId)
            ?? throw new KeyNotFoundException($"OC {ocId} no encontrada.");

        using var ms = new MemoryStream();
        await pdfStream.CopyToAsync(ms);
        oc.PDFAdjunto      = ms.ToArray();
        oc.PDFNombreArchivo = nombreArchivo;

        _repo.Update(oc);
        await _repo.SaveChangesAsync();
    }

    // ── HELPERS ──────────────────────────────────────────────────────
    private static void SincronizarDetalles(OrdenCompra oc, List<DetalleDto> dtos)
    {
        // Eliminar los que ya no están
        var idsNuevos = dtos.Where(d => d.DetalleId > 0).Select(d => d.DetalleId).ToHashSet();
        var aEliminar = oc.Detalles.Where(d => d.DetalleId > 0 && !idsNuevos.Contains(d.DetalleId)).ToList();
        foreach (var item in aEliminar) oc.Detalles.Remove(item);

        foreach (var dto in dtos)
        {
            if (dto.DetalleId == 0)
            {
                oc.Detalles.Add(new OrdenCompraDetalle
                {
                    CodigoProducto       = dto.CodigoProducto,
                    Descripcion          = dto.Descripcion,
                    Cantidad             = dto.Cantidad,
                    EstadoProductoId     = dto.EstadoProductoId,
                    MotivoEntregaParcial = dto.MotivoEntregaParcial
                });
            }
            else
            {
                var existente = oc.Detalles.First(d => d.DetalleId == dto.DetalleId);
                existente.CodigoProducto       = dto.CodigoProducto;
                existente.Descripcion          = dto.Descripcion;
                existente.Cantidad             = dto.Cantidad;
                existente.EstadoProductoId     = dto.EstadoProductoId;
                existente.MotivoEntregaParcial = dto.MotivoEntregaParcial;
            }
        }
    }

    public static DashboardItemDto MapToDashboard(OrdenCompra oc)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        int diasRestantes = oc.FechaSolicitada.DayNumber - hoy.DayNumber;

        string alerta = oc.EstadoOC.Nombre switch
        {
            "Cerrada" or "Cancelada" => "gris",
            _ when diasRestantes <= 0 => "rojo",
            _ when oc.TotalDiasEstablecidos > 0 &&
                   (double)diasRestantes / oc.TotalDiasEstablecidos.Value <= 0.20 => "rojo",
            _ when oc.TotalDiasEstablecidos > 0 &&
                   (double)diasRestantes / oc.TotalDiasEstablecidos.Value <= 0.40 => "amarillo",
            _ => "verde"
        };

        return new DashboardItemDto(
            OcId:                  oc.OcId,
            NumeroOC:              oc.NumeroOC,
            RazonSocial:           oc.Entidad.RazonSocial,
            Responsable:           oc.Responsable?.Nombre,
            EstadoOC:              oc.EstadoOC.Nombre,
            ColorEstado:           oc.EstadoOC.ColorHex,
            FechaSolicitada:       oc.FechaSolicitada,
            TotalDiasEstablecidos: oc.TotalDiasEstablecidos,
            DiasRestantes:         diasRestantes,
            AlertaNivel:           alerta,
            FechaLlegadaCorreo:    oc.FechaLlegadaCorreo,
            LugarEntrega:          oc.LugarEntrega,
            Departamento:          oc.Departamento,
            Movilidad:             oc.Movilidad
        );
    }
}
