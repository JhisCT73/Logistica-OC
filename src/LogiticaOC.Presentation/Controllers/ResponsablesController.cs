using LogiticaOC.Domain.Entities;
using LogiticaOC.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Presentation.Controllers;

public class ResponsablesController : Controller
{
    private readonly OcLogisticaDbContext _db;

    public ResponsablesController(OcLogisticaDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var responsables = await _db.Responsables
            .OrderBy(r => r.Nombre)
            .ToListAsync();

        return View(responsables);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre del responsable no puede estar vacío.";
            return RedirectToAction(nameof(Index));
        }

        nombre = nombre.Trim();

        var existe = await _db.Responsables
            .AnyAsync(r => r.Nombre.ToLower() == nombre.ToLower());

        if (existe)
        {
            TempData["Error"] = $"Ya existe un responsable registrado con el nombre '{nombre}'.";
            return RedirectToAction(nameof(Index));
        }

        var responsable = new Responsable
        {
            Nombre = nombre,
            Activo = true
        };

        _db.Responsables.Add(responsable);
        await _db.SaveChangesAsync();

        TempData["Exito"] = $"Responsable '{nombre}' creado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre del responsable no puede estar vacío.";
            return RedirectToAction(nameof(Index));
        }

        nombre = nombre.Trim();

        var responsable = await _db.Responsables.FindAsync(id);
        if (responsable == null)
        {
            TempData["Error"] = "El responsable solicitado no existe.";
            return RedirectToAction(nameof(Index));
        }

        var existe = await _db.Responsables
            .AnyAsync(r => r.ResponsableId != id && r.Nombre.ToLower() == nombre.ToLower());

        if (existe)
        {
            TempData["Error"] = $"Ya existe otro responsable registrado con el nombre '{nombre}'.";
            return RedirectToAction(nameof(Index));
        }

        responsable.Nombre = nombre;
        await _db.SaveChangesAsync();

        TempData["Exito"] = $"Nombre del responsable actualizado a '{nombre}' con éxito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var responsable = await _db.Responsables.FindAsync(id);
        if (responsable == null)
        {
            TempData["Error"] = "El responsable solicitado no existe.";
            return RedirectToAction(nameof(Index));
        }

        responsable.Activo = !responsable.Activo;
        await _db.SaveChangesAsync();

        var estadoStr = responsable.Activo ? "activado" : "desactivado";
        TempData["Exito"] = $"Responsable '{responsable.Nombre}' {estadoStr} con éxito.";
        return RedirectToAction(nameof(Index));
    }
}
