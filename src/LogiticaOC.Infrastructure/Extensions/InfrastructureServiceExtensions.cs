using LogiticaOC.Domain.Interfaces;
using LogiticaOC.Domain.Interfaces;
using LogiticaOC.Infrastructure.Repositories;
using LogiticaOC.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LogiticaOC.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Repositorios
        services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
        services.AddScoped<IEntidadRepository, EntidadRepository>();

        // Servicios de exportación
        services.AddScoped<IExportService, ExportService>();

        // Servicios de aplicación
        services.AddScoped<OrdenCompraService>();

        return services;
    }
}
