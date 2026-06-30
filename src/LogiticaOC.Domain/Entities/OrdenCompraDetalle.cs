namespace LogiticaOC.Domain.Entities;

public class OrdenCompraDetalle
{
    public int DetalleId { get; set; }

    public int OcId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;

    public string? CodigoProducto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }

    public int EstadoProductoId { get; set; }
    public EstadoProducto EstadoProducto { get; set; } = null!;

    /// <summary>
    /// Obligatorio solo cuando EstadoProducto == "Entrega Parcial".
    /// No registra cantidades — solo el motivo.
    /// </summary>
    public string? MotivoEntregaParcial { get; set; }
}
