namespace DocumentosFiscais.Web.Extensions;

/// <summary>
/// Extensões para configuração de logging da aplicação
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configura logging personalizado para a aplicação
    /// </summary>
    public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            // Configuração baseada no ambiente
            builder.ClearProviders();
            
            // Console sempre disponível
            builder.AddConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            });

            // Debug apenas em desenvolvimento
            builder.AddDebug();

            // Event Log apenas em produção (Windows)
            if (OperatingSystem.IsWindows())
            {
                builder.AddEventLog(options =>
                {
                    options.SourceName = "DocumentosFiscais";
                });
            }

            // Configurar níveis baseado no ambiente
            builder.SetMinimumLevel(LogLevel.Information);
            
            // Filtros específicos
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
            builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        });

        return services;
    }

    /// <summary>
    /// Middleware para logging de requisições importantes
    /// </summary>
    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await next();
            }
            finally
            {
                stopwatch.Stop();

                // Log apenas requisições importantes ou com problemas
                if (ShouldLogRequest(context))
                {
                    var level = GetLogLevel(context.Response.StatusCode);
                    var clientIP = GetClientIP(context);

                    logger.Log(level,
                        "🌐 {Method} {Path} - {StatusCode} - {Duration}ms - {ClientIP}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        clientIP);
                }
            }
        });

        return app;
    }

    /// <summary>
    /// Determina se a requisição deve ser logada
    /// </summary>
    private static bool ShouldLogRequest(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Sempre logar uploads
        if (path.Contains("/upload")) return true;
        
        // Sempre logar APIs
        if (path.StartsWith("/api/")) return true;
        
        // Sempre logar erros
        if (context.Response.StatusCode >= 400) return true;
        
        // Sempre logar requisições lentas (>1s)
        if (context.Items.ContainsKey("RequestStartTime"))
        {
            var startTime = (DateTime)context.Items["RequestStartTime"]!;
            if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(1))
                return true;
        }

        // Ignorar arquivos estáticos
        if (path.Contains("/css/") || path.Contains("/js/") || 
            path.Contains("/lib/") || path.Contains("/images/") ||
            path.Contains("/favicon.ico")) return false;

        return false;
    }

    /// <summary>
    /// Determina o nível de log baseado no status code
    /// </summary>
    private static LogLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }

    /// <summary>
    /// Obtém o IP do cliente
    /// </summary>
    private static string GetClientIP(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Configura logging estruturado para diferentes ambientes
    /// </summary>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Em desenvolvimento: logs mais verbosos e coloridos
            services.AddLogging(builder =>
            {
                builder.AddConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "[HH:mm:ss] ";
                });
            });
        }
        else
        {
            // Em produção: logs estruturados para análise
            services.AddLogging(builder =>
            {
                builder.AddJsonConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
                });
            });
        }

        return services;
    }
}