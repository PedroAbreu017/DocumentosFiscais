using DocumentosFiscais.Data;
using DocumentosFiscais.Data.Seed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DocumentosFiscais.Web.Extensions;

/// <summary>
/// Extensões para configuração e inicialização do banco de dados
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Inicializa o banco de dados e executa seed se necessário
    /// </summary>
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🔄 Iniciando configuração do banco de dados...");

            var context = scope.ServiceProvider.GetRequiredService<DocumentosContext>();

            // Verificar se o banco pode ser conectado
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogWarning("⚠️ Não foi possível conectar ao banco, tentando criar...");
            }

            // Garantir que o banco existe
            var created = await context.Database.EnsureCreatedAsync();
            
            if (created)
            {
                logger.LogInformation("🆕 Banco de dados criado com sucesso");
            }
            else
            {
                logger.LogInformation("✅ Banco de dados já existe e está disponível");
            }

            // Executar seed apenas em desenvolvimento
            if (app.Environment.IsDevelopment())
            {
                await ExecuteSeedDataAsync(scope.ServiceProvider, logger);
            }
            else
            {
                logger.LogInformation("🏭 Ambiente de produção - Seed ignorado");
            }

            logger.LogInformation("🚀 Banco de dados inicializado com sucesso!");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro crítico durante inicialização do banco de dados");
            
            // Em desenvolvimento, mostrar erro detalhado
            if (app.Environment.IsDevelopment())
            {
                logger.LogError("💡 Detalhes do erro: {ErrorDetails}", ex.ToString());
                logger.LogInformation("🔄 Aplicação continuará mesmo com erro no banco (desenvolvimento)");
            }
            else
            {
                // Em produção, interromper se banco não funcionar
                throw;
            }
        }

        return app;
    }

    /// <summary>
    /// Executa o seed de dados de desenvolvimento
    /// </summary>
    private static async Task ExecuteSeedDataAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            logger.LogInformation("🌱 Executando seed de dados de desenvolvimento...");

            var seedService = serviceProvider.GetRequiredService<SeedDataService>();
            await seedService.SeedAsync();

            logger.LogInformation("✅ Seed de dados executado com sucesso");

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ Erro durante execução do seed - aplicação continuará funcionando");
            
            // Não interromper a aplicação por erro no seed
            // Em desenvolvimento, é comum ter problemas de seed
        }
    }

    /// <summary>
    /// Adiciona serviços relacionados ao banco de dados
    /// </summary>
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Health Checks avançados com EntityFramework
        services.AddHealthChecks()
            .AddDbContextCheck<DocumentosContext>(
                name: "database",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "db", "sql", "entityframework" })
            .AddCheck("database-connection", () =>
            {
                try
                {
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DocumentosContext>();
                    
                    // Teste de conectividade simples
                    var canConnect = context.Database.CanConnect();
                    
                    return canConnect 
                        ? HealthCheckResult.Healthy("Database connection is healthy")
                        : HealthCheckResult.Degraded("Database connection is degraded");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("Database connection failed", ex);
                }
            }, tags: new[] { "db", "connection" });

        return services;
    }

    /// <summary>
    /// Configura endpoints de health check para o banco
    /// </summary>
    public static WebApplication UseDatabaseHealthCheck(this WebApplication app)
    {
        // Endpoint básico de health check
        app.MapHealthChecks("/health");
        
        // Endpoint específico para o banco
        app.MapHealthChecks("/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db"),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        exception = entry.Value.Exception?.Message,
                        duration = entry.Value.Duration.ToString()
                    }),
                    totalDuration = report.TotalDuration.ToString()
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });

        // Endpoint detalhado para desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTime.UtcNow,
                        environment = app.Environment.EnvironmentName,
                        checks = report.Entries.Select(entry => new
                        {
                            name = entry.Key,
                            status = entry.Value.Status.ToString(),
                            tags = entry.Value.Tags,
                            exception = entry.Value.Exception?.ToString(),
                            duration = entry.Value.Duration.TotalMilliseconds,
                            data = entry.Value.Data
                        }),
                        totalDuration = report.TotalDuration.TotalMilliseconds
                    };

                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    }));
                }
            });
        }
        
        return app;
    }

    /// <summary>
    /// Middleware para logging de performance do banco
    /// </summary>
    public static WebApplication UseDatabasePerformanceLogging(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.Use(async (context, next) =>
            {
                // Log queries lentas apenas em desenvolvimento
                if (context.Request.Path.StartsWithSegments("/api/") || 
                    context.Request.Path.StartsWithSegments("/Documentos"))
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    
                    await next();
                    
                    stopwatch.Stop();
                    
                    if (stopwatch.ElapsedMilliseconds > 1000) // Queries > 1s
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("🐌 Slow database operation detected: {Path} took {Duration}ms", 
                            context.Request.Path, stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                {
                    await next();
                }
            });
        }
        
        return app;
    }
}