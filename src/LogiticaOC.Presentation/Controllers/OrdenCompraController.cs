using LogiticaOC.Application.DTOs;
using LogiticaOC.Infrastructure.Services;
using LogiticaOC.Domain.Entities;
using LogiticaOC.Infrastructure.Data;
using LogiticaOC.Infrastructure.Services;
using LogiticaOC.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Presentation.Controllers;

public class OrdenCompraController : Controller
{
    private readonly OrdenCompraService _service;
    private readonly OcLogisticaDbContext _db;
    private readonly IExportService _export;

    public OrdenCompraController(OrdenCompraService service, OcLogisticaDbContext db, IExportService export)
    {
        _service = service;
        _db = db;
        _export = export;
    }

    // ── LISTADO ──────────────────────────────────────────────────────
    public async Task<IActionResult> Index(BusquedaVM filtros)
    {
        var ocs = await _db.OrdenesCompra
            .Include(o => o.Entidad)
            .Include(o => o.Responsable)
            .Include(o => o.EstadoOC)
            .Where(o =>
                (string.IsNullOrEmpty(filtros.NumeroOC) || o.NumeroOC.Contains(filtros.NumeroOC)) &&
                (!filtros.EntidadId.HasValue || o.EntidadId == filtros.EntidadId) &&
                (!filtros.ResponsableId.HasValue || o.ResponsableId == filtros.ResponsableId) &&
                (!filtros.EstadoOcId.HasValue || o.EstadoOcId == filtros.EstadoOcId) &&
                (!filtros.FechaDesde.HasValue || o.FechaLlegadaCorreo >= filtros.FechaDesde) &&
                (!filtros.FechaHasta.HasValue || o.FechaLlegadaCorreo <= filtros.FechaHasta)
            )
            .OrderByDescending(o => o.FechaRegistro)
            .ToListAsync();

        filtros.Responsables = new SelectList(await _db.Responsables.Where(r => r.Activo).ToListAsync(), "ResponsableId", "Nombre", filtros.ResponsableId);
        filtros.EstadosOC    = new SelectList(await _db.EstadosOC.ToListAsync(), "EstadoOcId", "Nombre", filtros.EstadoOcId);
        filtros.Entidades    = new SelectList(await _db.Entidades.Where(e => e.Activo).OrderBy(e => e.RazonSocial).ToListAsync(), "EntidadId", "RazonSocial", filtros.EntidadId);

        ViewBag.Filtros = filtros;
        var modelMapped = ocs.Select(o => OrdenCompraService.MapToDashboard(o)).ToList();

        if (Request.Headers["X-Requested-With-Partial"] == "true")
        {
            return PartialView("_OrdenCompraTable", modelMapped);
        }
        return View(modelMapped);
    }

    // ── DETALLE ──────────────────────────────────────────────────────
    public async Task<IActionResult> Detail(int id)
    {
        var oc = await _db.OrdenesCompra
            .Include(o => o.Entidad)
            .Include(o => o.Responsable)
            .Include(o => o.EstadoOC)
            .Include(o => o.Detalles).ThenInclude(d => d.EstadoProducto)
            .Include(o => o.Guias)
            .Include(o => o.Historial)
            .FirstOrDefaultAsync(o => o.OcId == id);

        if (oc is null) return NotFound();

        ViewBag.EstadoNombre  = oc.EstadoOC.Nombre;
        ViewBag.ColorEstado   = oc.EstadoOC.ColorHex;
        ViewBag.FechaRegistro = oc.FechaRegistro;

        var vm = await MapToFormVM(oc);
        vm.Historial = oc.Historial.OrderByDescending(h => h.FechaModificacion).ToList();
        return View(vm);
    }

    // ── CREAR GET ────────────────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        var vm = new OrdenCompraFormVM
        {
            FechaLlegadaCorreo = DateOnly.FromDateTime(DateTime.Today),
            FechaSolicitada = DateOnly.FromDateTime(DateTime.Today),
            EstadoOcId = 1 // Pendiente
        };
        await PopularSelectListsAsync(vm);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_FormCreatePartial", vm);
        }
        return View(vm);
    }

    // ── CREAR POST ───────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrdenCompraFormVM vm, IFormFile? pdfFile)
    {
        var esAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid) 
        { 
            await PopularSelectListsAsync(vm); 
            return esAjax ? PartialView("_FormCreatePartial", vm) : View(vm); 
        }

        try
        {
            var dto = new CrearOcDto(
                vm.NumeroOC, vm.RazonSocialEntidad, vm.ResponsableId, vm.EstadoOcId,
                vm.FechaLlegadaCorreo, vm.FechaSolicitada,
                vm.TotalDiasEstablecidos, vm.LugarEntrega, vm.Departamento,
                vm.Movilidad, vm.Observaciones,
                vm.Detalles.Select(d => new DetalleDto(
                    d.DetalleId, d.CodigoProducto, d.Descripcion,
                    d.Cantidad, d.EstadoProductoId, d.MotivoEntregaParcial)).ToList()
            );

            int ocId = await _service.CrearAsync(dto);

            if (pdfFile is { Length: > 0 })
                await _service.GuardarPdfAsync(ocId, pdfFile.OpenReadStream(), pdfFile.FileName);

            if (esAjax)
            {
                return Json(new { success = true, message = $"OC {vm.NumeroOC} creada correctamente." });
            }

            TempData["Exito"] = $"OC {vm.NumeroOC} creada correctamente.";
            return RedirectToAction(nameof(Detail), new { id = ocId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.NumeroOC), ex.Message);
            await PopularSelectListsAsync(vm);
            return esAjax ? PartialView("_FormCreatePartial", vm) : View(vm);
        }
    }

    // ── EDITAR GET ───────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id, string? returnUrl = null)
    {
        var oc = await _db.OrdenesCompra
            .Include(o => o.Entidad)
            .Include(o => o.Responsable)
            .Include(o => o.EstadoOC)
            .Include(o => o.Detalles).ThenInclude(d => d.EstadoProducto)
            .Include(o => o.Guias)
            .FirstOrDefaultAsync(o => o.OcId == id);

        if (oc is null) return NotFound();
        var vm = await MapToFormVM(oc);
        await PopularSelectListsAsync(vm);
        ViewBag.ReturnUrl = returnUrl;
        return View(vm);
    }

    // ── EDITAR POST ──────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrdenCompraFormVM vm, IFormFile? pdfFile, string? observacion, string? returnUrl = null)
    {
        var esAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid) 
        { 
            await PopularSelectListsAsync(vm); 
            ViewBag.ReturnUrl = returnUrl;
            return View(vm); 
        }

        var dto = new EditarOcDto(
            vm.OcId, vm.ResponsableId, vm.EstadoOcId,
            vm.FechaLlegadaCorreo, vm.FechaSolicitada,
            vm.FechaDespacho, vm.FechaCierre,
            vm.TotalDiasEstablecidos, vm.LugarEntrega, vm.Departamento,
            vm.Movilidad, vm.Observaciones,
            vm.Detalles.Select(d => new DetalleDto(
                d.DetalleId, d.CodigoProducto, d.Descripcion,
                d.Cantidad, d.EstadoProductoId, d.MotivoEntregaParcial)).ToList()
        );

        await _service.EditarAsync(dto, observacion);

        if (pdfFile is { Length: > 0 })
            await _service.GuardarPdfAsync(vm.OcId, pdfFile.OpenReadStream(), pdfFile.FileName);

        if (esAjax)
        {
            var redirectTarget = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action(nameof(Detail), new { id = vm.OcId });
            return Json(new { success = true, message = "OC actualizada correctamente.", redirectUrl = redirectTarget });
        }

        TempData["Exito"] = "OC actualizada correctamente.";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Detail), new { id = vm.OcId });
    }

    // ── CANCELAR POST ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string motivo)
    {
        await _service.CancelarAsync(id, motivo);
        TempData["Exito"] = "OC cancelada.";
        return RedirectToAction(nameof(Index));
    }

    // ── AUTOCOMPLETE ENTIDADES ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> AutocompleteEntidades(string term)
    {
        var entidades = await _db.Entidades
            .Where(e => e.Activo && e.RazonSocial.Contains(term))
            .OrderBy(e => e.RazonSocial)
            .Take(10)
            .Select(e => new { label = e.RazonSocial, value = e.RazonSocial })
            .ToListAsync();
        return Json(entidades);
    }

    // ── AGREGAR GUÍA (AJAX POST) ──────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> AgregarGuia(int ocId, GuiaVM guiaVm)
    {
        var guia = new Guia
        {
            OcId        = ocId,
            NumeroGuia  = guiaVm.NumeroGuia,
            FechaGuia   = guiaVm.FechaGuia,
            Responsable = guiaVm.Responsable
        };
        _db.Guias.Add(guia);
        await _db.SaveChangesAsync();
        return Ok(new { guia.GuiaId });
    }

    // ── ACTUALIZAR FECHA RÁPIDA (AJAX POST) ──────────────────────────
    [HttpPost]
    public async Task<IActionResult> ActualizarFechaRapida(int ocId, string campo, DateOnly? fecha)
    {
        var oc = await _db.OrdenesCompra
            .Include(o => o.EstadoOC)
            .Include(o => o.Responsable)
            .FirstOrDefaultAsync(o => o.OcId == ocId);

        if (oc == null) return NotFound(new { message = "Orden de Compra no encontrada." });

        string mensaje = "";
        string observacionHistorial = "";

        if (campo == "FechaDespacho")
        {
            var anteriorStr = oc.FechaDespacho?.ToString("dd/MM/yyyy") ?? "vacía";
            oc.FechaDespacho = fecha;
            var nuevaStr = fecha?.ToString("dd/MM/yyyy") ?? "vacía";
            mensaje = $"Fecha de despacho actualizada a: {nuevaStr}.";
            observacionHistorial = $"Fecha de despacho modificada de {anteriorStr} a {nuevaStr}.";
        }
        else if (campo == "FechaCierre")
        {
            var anteriorStr = oc.FechaCierre?.ToString("dd/MM/yyyy") ?? "vacía";
            oc.FechaCierre = fecha;
            var nuevaStr = fecha?.ToString("dd/MM/yyyy") ?? "vacía";
            mensaje = $"Fecha de cierre actualizada a: {nuevaStr}.";
            observacionHistorial = $"Fecha de cierre modificada de {anteriorStr} a {nuevaStr}.";
        }
        else
        {
            return BadRequest(new { message = "Campo no soportado." });
        }

        // Agregar al historial
        var historial = new HistorialOC
        {
            OcId = ocId,
            EstadoAnterior = oc.EstadoOC?.Nombre,
            EstadoNuevo = oc.EstadoOC?.Nombre,
            ResponsableAnterior = oc.Responsable?.Nombre,
            ResponsableNuevo = oc.Responsable?.Nombre,
            Observacion = observacionHistorial,
            FechaModificacion = DateTime.UtcNow
        };
        _db.HistorialOC.Add(historial);

        await _db.SaveChangesAsync();
        return Json(new { success = true, message = mensaje });
    }

    // ── DESCARGAR PDF ADJUNTO ─────────────────────────────────────────
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var oc = await _db.OrdenesCompra.FindAsync(id);
        if (oc?.PDFAdjunto is null) return NotFound();
        return File(oc.PDFAdjunto, "application/pdf", oc.PDFNombreArchivo ?? $"OC_{oc.NumeroOC}.pdf");
    }

    // ── EXPORT PDF OC ─────────────────────────────────────────────────
    public async Task<IActionResult> ExportarPdf(int id)
    {
        var bytes = await _export.GenerarOcPdfAsync(id);
        return File(bytes, "application/pdf", $"OC_{id}.pdf");
    }

    // ── HOJA DE RUTA ─────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> GenerarHojaRuta()
    {
        var estadosExcluidos = new[] { "Cerrada", "Cancelada", "Listo para Despacho" };
        var tienePendientes = await _db.OrdenesCompra
            .AnyAsync(oc => !estadosExcluidos.Contains(oc.EstadoOC.Nombre));

        if (!tienePendientes)
        {
            TempData["Error"] = "No hay órdenes de compra pendientes en este momento para generar la hoja de ruta.";
            return RedirectToAction("Index");
        }

        var bytes = await _service.GenerarHojaRutaAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"HojaRuta_{DateTime.Today:yyyyMMdd}.xlsx");
    }

    // ── HELPERS ──────────────────────────────────────────────────────
    private async Task PopularSelectListsAsync(OrdenCompraFormVM vm)
    {
        vm.Responsables    = new SelectList(await _db.Responsables.Where(r => r.Activo).ToListAsync(), "ResponsableId", "Nombre", vm.ResponsableId);
        vm.EstadosOC       = new SelectList(await _db.EstadosOC.ToListAsync(), "EstadoOcId", "Nombre", vm.EstadoOcId);
        vm.EstadosProducto = new SelectList(await _db.EstadosProducto.ToListAsync(), "EstadoProductoId", "Nombre");
    }

    private async Task<OrdenCompraFormVM> MapToFormVM(OrdenCompra oc)
    {
        await Task.CompletedTask;
        return new OrdenCompraFormVM
        {
            OcId                   = oc.OcId,
            NumeroOC               = oc.NumeroOC,
            RazonSocialEntidad     = oc.Entidad.RazonSocial,
            ResponsableId          = oc.ResponsableId,
            EstadoOcId             = oc.EstadoOcId,
            FechaLlegadaCorreo     = oc.FechaLlegadaCorreo,
            FechaSolicitada        = oc.FechaSolicitada,
            FechaDespacho          = oc.FechaDespacho,
            FechaCierre            = oc.FechaCierre,
            TotalDiasEstablecidos  = oc.TotalDiasEstablecidos,
            LugarEntrega           = oc.LugarEntrega,
            Departamento           = oc.Departamento,
            Movilidad              = oc.Movilidad,
            Observaciones          = oc.Observaciones,
            PDFNombreArchivo       = oc.PDFNombreArchivo,
            Detalles               = oc.Detalles.Select(d => new DetalleVM
            {
                DetalleId            = d.DetalleId,
                CodigoProducto       = d.CodigoProducto,
                Descripcion          = d.Descripcion,
                Cantidad             = d.Cantidad,
                EstadoProductoId     = d.EstadoProductoId,
                MotivoEntregaParcial = d.MotivoEntregaParcial,
                EstadoNombre         = d.EstadoProducto?.Nombre ?? "",
                EstadoColor          = d.EstadoProducto?.ColorHex ?? "#6c757d"
            }).ToList(),
            Guias = oc.Guias.Select(g => new GuiaVM
            {
                GuiaId     = g.GuiaId,
                NumeroGuia = g.NumeroGuia,
                FechaGuia  = g.FechaGuia,
                Responsable = g.Responsable
            }).ToList()
        };
    }
}
