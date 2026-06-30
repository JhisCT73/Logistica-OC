namespace LogiticaOC.Domain.Entities;

/// <summary>
/// Estados del ciclo de vida de la OC completa.
/// Seed: Pendiente | Listo para Despacho | Cerrada | Cancelada
/// </summary>
public class EstadoOC
{
    public int EstadoOcId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#6c757d";

    // Navegación
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
}
