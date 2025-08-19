using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace DocumentosFiscais.Web.Extensions;

/// <summary>
/// Extensões para configuração de segurança da aplicação
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    /// Adiciona todos os serviços de segurança necessários
    /// </summary>
    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services)
    {
        // CORS Policy
        services.AddCors(options =>
        {
            options.AddPolicy("AllowUpload", policy =>
            {
                policy.WithOrigins("http://localhost:5128", "https://localhost:5128")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });

            options.AddPolicy("RestrictedUpload", policy =>
            {
                policy.WithOrigins("http://localhost:5128", "https://localhost:5128")
                      .WithMethods("POST", "OPTIONS")
                      .WithHeaders("Content-Type", "Accept", "X-Requested-With")
                      .AllowCredentials();
            });
        });

        // Rate Limiting
        services.AddRateLimiter(options =>
        {
            // Policy para uploads
            options.AddFixedWindowLimiter("UploadPolicy", config =>
            {
                config.PermitLimit = 10; // 10 uploads por minuto
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 5;
            });

            // Policy para APIs
            options.AddFixedWindowLimiter("ApiPolicy", config =>
            {
                config.PermitLimit = 100; // 100 requests por minuto
                config.Window = TimeSpan.FromMinutes(1);
            });

            // Policy global mais permissiva
            options.AddFixedWindowLimiter("GlobalPolicy", config =>
            {
                config.PermitLimit = 1000; // 1000 requests por minuto
                config.Window = TimeSpan.FromMinutes(1);
            });
        });

        // HSTS Configuration
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        // Antiforgery (opcional para compatibilidade)
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.SuppressXFrameOptionsHeader = false;
        });

        return services;
    }

    /// <summary>
    /// Configura o pipeline de segurança da aplicação
    /// </summary>
    public static WebApplication ConfigureSecurityPipeline(this WebApplication app)
    {
        // Security Headers Middleware
        app.UseSecurityHeaders();

        // Rate Limiting
        app.UseRateLimiter();

        // Logging de segurança para uploads
        app.UseUploadLogging();

        return app;
    }

    /// <summary>
    /// Middleware para headers de segurança
    /// </summary>
    private static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            // Headers básicos de segurança
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy otimizada
            var csp = string.Join("; ", new[]
            {
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com",
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com",
                "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com data:",
                "img-src 'self' data: https:",
                "connect-src 'self'",
                "object-src 'none'",
                "base-uri 'self'"
            });

            context.Response.Headers.Append("Content-Security-Policy", csp);

            await next();
        });

        return app;
    }

    /// <summary>
    /// Middleware para logging de uploads
    /// </summary>
    private static WebApplication UseUploadLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            // Log apenas requisições de upload
            if (context.Request.Path.StartsWithSegments("/Documentos/Upload"))
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                
                var clientIP = GetClientIPAddress(context);
                var userAgent = context.Request.Headers.UserAgent.ToString();
                
                logger.LogInformation(
                    "📤 Upload request from {ClientIP} - {UserAgent} - {Method} {Path}",
                    clientIP, userAgent, context.Request.Method, context.Request.Path);
            }

            await next();
        });

        return app;
    }

    /// <summary>
    /// Obtém o endereço IP real do cliente
    /// </summary>
    private static string GetClientIPAddress(HttpContext context)
    {
        // Verificar headers de proxy
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIP))
        {
            return xRealIP;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}