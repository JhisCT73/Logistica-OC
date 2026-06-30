using LogiticaOC.Domain.Entities;

namespace LogiticaOC.Domain.Interfaces;

public interface IEntidadRepository
{
    Task<IEnumerable<Entidad>> BuscarPorNombreAsync(string termino);
    Task<Entidad?> GetByRazonSocialAsync(string razonSocial);
    Task<Entidad> ObtenerOCrearAsync(string razonSocial);
    Task<IEnumerable<Entidad>> GetAllActivasAsync();
}
