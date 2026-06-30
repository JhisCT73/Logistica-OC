using LogiticaOC.Domain.Entities;
using LogiticaOC.Domain.Interfaces;
using LogiticaOC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Infrastructure.Repositories;

public class EntidadRepository : IEntidadRepository
{
    private readonly OcLogisticaDbContext _db;

    public EntidadRepository(OcLogisticaDbContext db) => _db = db;

    public async Task<IEnumerable<Entidad>> BuscarPorNombreAsync(string termino) =>
        await _db.Entidades
            .Where(e => e.Activo && e.RazonSocial.Contains(termino))
            .OrderBy(e => e.RazonSocial)
            .Take(10)
            .ToListAsync();

    public async Task<Entidad?> GetByRazonSocialAsync(string razonSocial) =>
        await _db.Entidades.FirstOrDefaultAsync(e => e.RazonSocial == razonSocial);

    public async Task<Entidad> ObtenerOCrearAsync(string razonSocial)
    {
        var existente = await GetByRazonSocialAsync(razonSocial);
        if (existente is not null) return existente;

        var nueva = new Entidad { RazonSocial = razonSocial.Trim(), Activo = true };
        await _db.Entidades.AddAsync(nueva);
        await _db.SaveChangesAsync();
        return nueva;
    }

    public async Task<IEnumerable<Entidad>> GetAllActivasAsync() =>
        await _db.Entidades
            .Where(e => e.Activo)
            .OrderBy(e => e.RazonSocial)
            .ToListAsync();
}
