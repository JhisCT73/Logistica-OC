namespace LogiticaOC.Domain.Entities;

/// <summary>
/// Estados del ciclo de vida de cada producto dentro de una OC.
/// Seed: En Proceso | Urgente Compras | Importación | Entrega Parcial | Cerrada | Cancelada
/// </summary>
public class EstadoProducto
{
    public int EstadoProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#6c757d";

    // Navegación
    public ICollection<OrdenCompraDetalle> Detalles { get; set; } = new List<OrdenCompraDetalle>();
}
