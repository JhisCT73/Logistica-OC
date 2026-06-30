namespace LogiticaOC.Domain.Entities;

public class Responsable
{
    public int ResponsableId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
}
