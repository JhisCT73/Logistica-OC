namespace LogiticaOC.Domain.Entities;

public class Entidad
{
    public int EntidadId { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
}
