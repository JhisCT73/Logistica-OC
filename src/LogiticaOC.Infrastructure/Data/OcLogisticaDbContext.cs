using LogiticaOC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiticaOC.Infrastructure.Data;

public class OcLogisticaDbContext : DbContext
{
    public OcLogisticaDbContext(DbContextOptions<OcLogisticaDbContext> options) : base(options) { }

    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraDetalle> OrdenesCompraDetalle => Set<OrdenCompraDetalle>();
    public DbSet<Entidad> Entidades => Set<Entidad>();
    public DbSet<Responsable> Responsables => Set<Responsable>();
    public DbSet<EstadoOC> EstadosOC => Set<EstadoOC>();
    public DbSet<EstadoProducto> EstadosProducto => Set<EstadoProducto>();
    public DbSet<Guia> Guias => Set<Guia>();
    public DbSet<HistorialOC> HistorialOC => Set<HistorialOC>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones Fluent API del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OcLogisticaDbContext).Assembly);

        // ── Seed: EstadosOC ──────────────────────────────────────────────
        modelBuilder.Entity<EstadoOC>().HasData(
            new EstadoOC { EstadoOcId = 1, Nombre = "Pendiente",           ColorHex = "#4f8ef7" },
            new EstadoOC { EstadoOcId = 2, Nombre = "Listo para Despacho", ColorHex = "#6f42c1" },
            new EstadoOC { EstadoOcId = 3, Nombre = "Entrega Parcial",     ColorHex = "#f59e0b" },
            new EstadoOC { EstadoOcId = 4, Nombre = "Cerrada",             ColorHex = "#22c55e" },
            new EstadoOC { EstadoOcId = 5, Nombre = "Cancelada",           ColorHex = "#6c757d" }
        );

        // ── Seed: EstadosProducto ────────────────────────────────────────
        modelBuilder.Entity<EstadoProducto>().HasData(
            new EstadoProducto { EstadoProductoId = 1, Nombre = "En Proceso",      ColorHex = "#28a745" },
            new EstadoProducto { EstadoProductoId = 2, Nombre = "Urgente Compras", ColorHex = "#fd7e14" },
            new EstadoProducto { EstadoProductoId = 3, Nombre = "Importación",     ColorHex = "#0dcaf0" },
            new EstadoProducto { EstadoProductoId = 4, Nombre = "Entrega Parcial", ColorHex = "#ffc107" },
            new EstadoProducto { EstadoProductoId = 5, Nombre = "Cerrada",         ColorHex = "#6c757d" },
            new EstadoProducto { EstadoProductoId = 6, Nombre = "Cancelada",       ColorHex = "#343a40" }
        );
    }
}
