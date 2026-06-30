using LogiticaOC.Application.DTOs;
using LogiticaOC.Infrastructure.Services;
using LogiticaOC.Infrastructure.Data;
using LogiticaOC.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Presentation.Controllers;

public class DashboardController : Controller
{
    private readonly OrdenCompraService _service;
    private readonly OcLogisticaDbContext _db;

    public DashboardController(OrdenCompraService service, OcLogisticaDbContext db)
    {
        _service = service;
        _db = db;
    }

    public async Task<IActionResult> Index(BusquedaVM? filtros)
    {
        filtros ??= new BusquedaVM();

        var queryDb = _db.OrdenesCompra
            .Include(o => o.Entidad)
            .Include(o => o.Responsable)
            .Include(o => o.EstadoOC)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(filtros.NumeroOC))
            queryDb = queryDb.Where(o => o.NumeroOC.Contains(filtros.NumeroOC));

        if (filtros.EntidadId.HasValue)
            queryDb = queryDb.Where(o => o.EntidadId == filtros.EntidadId);

        if (filtros.ResponsableId.HasValue)
            queryDb = queryDb.Where(o => o.ResponsableId == filtros.ResponsableId);

        if (filtros.EstadoOcId.HasValue)
            queryDb = queryDb.Where(o => o.EstadoOcId == filtros.EstadoOcId);

        if (filtros.FechaDesde.HasValue)
            queryDb = queryDb.Where(o => o.FechaLlegadaCorreo >= filtros.FechaDesde);

        if (filtros.FechaHasta.HasValue)
            queryDb = queryDb.Where(o => o.FechaLlegadaCorreo <= filtros.FechaHasta);

        var ocsList = await queryDb
            .OrderByDescending(o => o.FechaRegistro)
            .ToListAsync();

        var lista = ocsList.Select(o => OrdenCompraService.MapToDashboard(o)).ToList();

        // Métricas
        ViewBag.TotalPendientes  = lista.Count(o => o.EstadoOC is not ("Cerrada" or "Cancelada"));
        ViewBag.TotalEntregadas  = lista.Count(o => o.EstadoOC is "Cerrada" or "Entrega Parcial");
        ViewBag.TotalCriticas    = lista.Count(o => o.AlertaNivel == "rojo");

        // OC por Cliente
        ViewBag.ClientesMetricas = ocsList
            .GroupBy(o => o.Entidad.RazonSocial)
            .Select(g => new ClienteMetricasDto(
                RazonSocial: g.Key,
                TotalOcs:    g.Count(),
                Pendientes:  g.Count(o => o.EstadoOC.Nombre is not ("Cerrada" or "Cancelada"))
            ))
            .OrderByDescending(c => c.TotalOcs)
            .ToList();

        filtros.Responsables = new SelectList(await _db.Responsables.Where(r => r.Activo).ToListAsync(), "ResponsableId", "Nombre", filtros.ResponsableId);
        filtros.EstadosOC    = new SelectList(await _db.EstadosOC.ToListAsync(), "EstadoOcId", "Nombre", filtros.EstadoOcId);
        filtros.Entidades    = new SelectList(await _db.Entidades.Where(e => e.Activo).OrderBy(e => e.RazonSocial).ToListAsync(), "EntidadId", "RazonSocial", filtros.EntidadId);

        ViewBag.Items   = lista;
        ViewBag.Filtros = filtros;

        if (Request.Headers["X-Requested-With-Partial"] == "true")
        {
            return PartialView("_DashboardContent", lista);
        }
        return View(lista);
    }
}
