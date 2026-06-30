namespace LogiticaOC.Domain.Entities;

public class Guia
{
    public int GuiaId { get; set; }

    public int OcId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;

    public string NumeroGuia { get; set; } = string.Empty;
    public DateOnly FechaGuia { get; set; }
    public string? Responsable { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
