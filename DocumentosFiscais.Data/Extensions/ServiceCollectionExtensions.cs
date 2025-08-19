using DocumentosFiscais.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DocumentosFiscais.Core.Interfaces; 

namespace DocumentosFiscais.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<DocumentosContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Repository Pattern
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        
        return services;
    }
}