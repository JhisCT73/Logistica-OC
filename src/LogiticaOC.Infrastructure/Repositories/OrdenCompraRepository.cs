using LogiticaOC.Domain.Entities;
using LogiticaOC.Domain.Interfaces;
using LogiticaOC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Infrastructure.Repositories;

public class OrdenCompraRepository : IOrdenCompraRepository
{
    private readonly OcLogisticaDbContext _db;

    public OrdenCompraRepository(OcLogisticaDbContext db) => _db = db;

    public async Task<OrdenCompra?> GetByIdAsync(int id, bool includeDetalles = false, bool includeGuias = false, bool includeHistorial = false)
    {
        var query = _db.OrdenesCompra
            .Include(x => x.Entidad)
            .Include(x => x.Responsable)
            .Include(x => x.EstadoOC)
            .AsQueryable();

        if (includeDetalles) query = query.Include(x => x.Detalles).ThenInclude(d => d.EstadoProducto);
        if (includeGuias) query = query.Include(x => x.Guias);
        if (includeHistorial) query = query.Include(x => x.Historial);

        return await query.FirstOrDefaultAsync(x => x.OcId == id);
    }

    public async Task<IEnumerable<OrdenCompra>> GetAllAsync() =>
        await _db.OrdenesCompra
            .Include(x => x.Entidad)
            .Include(x => x.Responsable)
            .Include(x => x.EstadoOC)
            .OrderByDescending(x => x.FechaRegistro)
            .ToListAsync();

    public async Task<IEnumerable<OrdenCompra>> BuscarAsync(
        string? numeroOc, int? entidadId, int? responsableId,
        int? estadoOcId, DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var query = _db.OrdenesCompra
            .Include(x => x.Entidad)
            .Include(x => x.Responsable)
            .Include(x => x.EstadoOC)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(numeroOc))
            query = query.Where(x => x.NumeroOC.Contains(numeroOc));

        if (entidadId.HasValue)
            query = query.Where(x => x.EntidadId == entidadId.Value);

        if (responsableId.HasValue)
            query = query.Where(x => x.ResponsableId == responsableId.Value);

        if (estadoOcId.HasValue)
            query = query.Where(x => x.EstadoOcId == estadoOcId.Value);

        if (fechaDesde.HasValue)
            query = query.Where(x => x.FechaLlegadaCorreo >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(x => x.FechaLlegadaCorreo <= fechaHasta.Value);

        return await query.OrderByDescending(x => x.FechaRegistro).ToListAsync();
    }

    public async Task<bool> ExisteNumeroOCAsync(string numeroOC) =>
        await _db.OrdenesCompra.AnyAsync(x => x.NumeroOC == numeroOC);

    public async Task AddAsync(OrdenCompra oc) => await _db.OrdenesCompra.AddAsync(oc);

    public void Update(OrdenCompra oc) => _db.OrdenesCompra.Update(oc);

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
