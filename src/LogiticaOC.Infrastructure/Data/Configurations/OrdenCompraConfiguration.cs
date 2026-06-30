using LogiticaOC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiticaOC.Infrastructure.Data.Configurations;

public class OrdenCompraConfiguration : IEntityTypeConfiguration<OrdenCompra>
{
    public void Configure(EntityTypeBuilder<OrdenCompra> builder)
    {
        builder.ToTable("OrdenCompra");
        builder.HasKey(x => x.OcId);

        builder.Property(x => x.NumeroOC)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.NumeroOC)
            .IsUnique();

        builder.Property(x => x.LugarEntrega).HasMaxLength(200);
        builder.Property(x => x.Departamento).HasMaxLength(100);
        builder.Property(x => x.Movilidad).HasMaxLength(200);
        builder.Property(x => x.PDFNombreArchivo).HasMaxLength(255);
        builder.Property(x => x.PDFAdjunto).HasColumnType("varbinary(max)");
        builder.Property(x => x.FechaRegistro).HasDefaultValueSql("GETUTCDATE()");

        // Relaciones
        builder.HasOne(x => x.Entidad)
            .WithMany(e => e.OrdenesCompra)
            .HasForeignKey(x => x.EntidadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Responsable)
            .WithMany(r => r.OrdenesCompra)
            .HasForeignKey(x => x.ResponsableId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.EstadoOC)
            .WithMany(e => e.OrdenesCompra)
            .HasForeignKey(x => x.EstadoOcId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Detalles)
            .WithOne(d => d.OrdenCompra)
            .HasForeignKey(d => d.OcId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Guias)
            .WithOne(g => g.OrdenCompra)
            .HasForeignKey(g => g.OcId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Historial)
            .WithOne(h => h.OrdenCompra)
            .HasForeignKey(h => h.OcId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.EntidadId);
        builder.HasIndex(x => x.ResponsableId);
        builder.HasIndex(x => x.EstadoOcId);
        builder.HasIndex(x => x.FechaRegistro);
    }
}
