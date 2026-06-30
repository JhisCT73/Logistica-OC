namespace LogiticaOC.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEn { get; set; }
}
