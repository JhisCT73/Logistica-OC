using LogiticaOC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiticaOC.Infrastructure.Data.Configurations;

public class EntidadConfiguration : IEntityTypeConfiguration<Entidad>
{
    public void Configure(EntityTypeBuilder<Entidad> builder)
    {
        builder.ToTable("Entidades");
        builder.HasKey(x => x.EntidadId);
        builder.Property(x => x.RazonSocial).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreadoEn).HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(x => x.RazonSocial);
    }
}

public class ResponsableConfiguration : IEntityTypeConfiguration<Responsable>
{
    public void Configure(EntityTypeBuilder<Responsable> builder)
    {
        builder.ToTable("Responsables");
        builder.HasKey(x => x.ResponsableId);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
    }
}

public class EstadoOCConfiguration : IEntityTypeConfiguration<EstadoOC>
{
    public void Configure(EntityTypeBuilder<EstadoOC> builder)
    {
        builder.ToTable("EstadosOC");
        builder.HasKey(x => x.EstadoOcId);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ColorHex).IsRequired().HasMaxLength(7);
    }
}

public class EstadoProductoConfiguration : IEntityTypeConfiguration<EstadoProducto>
{
    public void Configure(EntityTypeBuilder<EstadoProducto> builder)
    {
        builder.ToTable("EstadosProducto");
        builder.HasKey(x => x.EstadoProductoId);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ColorHex).IsRequired().HasMaxLength(7);
    }
}

public class OrdenCompraDetalleConfiguration : IEntityTypeConfiguration<OrdenCompraDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenCompraDetalle> builder)
    {
        builder.ToTable("OrdenCompraDetalle");
        builder.HasKey(x => x.DetalleId);
        builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(300);
        builder.Property(x => x.CodigoProducto).HasMaxLength(50);
        builder.Property(x => x.Cantidad).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MotivoEntregaParcial).HasMaxLength(500);
        builder.HasIndex(x => x.OcId);
    }
}

public class GuiaConfiguration : IEntityTypeConfiguration<Guia>
{
    public void Configure(EntityTypeBuilder<Guia> builder)
    {
        builder.ToTable("Guias");
        builder.HasKey(x => x.GuiaId);
        builder.Property(x => x.NumeroGuia).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Responsable).HasMaxLength(150);
        builder.Property(x => x.CreadoEn).HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(x => x.OcId);
    }
}

public class HistorialOCConfiguration : IEntityTypeConfiguration<HistorialOC>
{
    public void Configure(EntityTypeBuilder<HistorialOC> builder)
    {
        builder.ToTable("HistorialOC");
        builder.HasKey(x => x.HistorialId);
        builder.Property(x => x.EstadoAnterior).HasMaxLength(100);
        builder.Property(x => x.EstadoNuevo).HasMaxLength(100);
        builder.Property(x => x.ResponsableAnterior).HasMaxLength(100);
        builder.Property(x => x.ResponsableNuevo).HasMaxLength(100);
        builder.Property(x => x.Observacion).HasMaxLength(500);
        builder.Property(x => x.FechaModificacion).HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(x => x.OcId);
    }
}
