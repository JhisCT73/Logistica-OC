using LogiticaOC.Domain.Entities;

namespace LogiticaOC.Domain.Interfaces;

public interface IOrdenCompraRepository
{
    Task<OrdenCompra?> GetByIdAsync(int id, bool includeDetalles = false, bool includeGuias = false, bool includeHistorial = false);
    Task<IEnumerable<OrdenCompra>> GetAllAsync();
    Task<IEnumerable<OrdenCompra>> BuscarAsync(string? numeroOc, int? entidadId, int? responsableId, int? estadoOcId, DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<bool> ExisteNumeroOCAsync(string numeroOC);
    Task AddAsync(OrdenCompra oc);
    void Update(OrdenCompra oc);
    Task SaveChangesAsync();
}
