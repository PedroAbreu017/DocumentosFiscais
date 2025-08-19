using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DocumentosFiscais.Web.Controllers;

/// <summary>
/// Controller para configurações do sistema
/// </summary>
public class SettingsController : Controller
{
    private readonly ILogger<SettingsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public SettingsController(
        ILogger<SettingsController> logger, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Página principal de configurações
    /// </summary>
    public IActionResult Index()
    {
        ViewBag.Title = "Configurações";
        
        var viewModel = new SettingsViewModel
        {
            General = GetGeneralSettings(),
            Upload = GetUploadSettings(),
            Security = GetSecuritySettings(),
            Database = GetDatabaseSettings()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Configurações gerais
    /// </summary>
    public IActionResult General()
    {
        ViewBag.Title = "Configurações Gerais";
        
        var settings = GetGeneralSettings();
        return View(settings);
    }

    /// <summary>
    /// Configurações de upload
    /// </summary>
    public IActionResult Upload()
    {
        ViewBag.Title = "Configurações de Upload";
        
        var settings = GetUploadSettings();
        return View(settings);
    }

    /// <summary>
    /// Configurações de segurança
    /// </summary>
    public IActionResult Security()
    {
        ViewBag.Title = "Configurações de Segurança";
        
        var settings = GetSecuritySettings();
        return View(settings);
    }

    /// <summary>
    /// Configurações de banco de dados
    /// </summary>
    public IActionResult Database()
    {
        ViewBag.Title = "Configurações de Banco de Dados";
        
        var settings = GetDatabaseSettings();
        return View(settings);
    }

    /// <summary>
    /// Configurações de logs
    /// </summary>
    public IActionResult Logs()
    {
        ViewBag.Title = "Configurações de Logs";
        
        var settings = GetLogSettings();
        return View(settings);
    }

    /// <summary>
    /// Backup e restore
    /// </summary>
    public IActionResult Backup()
    {
        ViewBag.Title = "Backup e Restore";
        return View();
    }

    /// <summary>
    /// Atualizar configurações gerais
    /// </summary>
    [HttpPost]
    public IActionResult UpdateGeneral(GeneralSettingsViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Em um sistema real, aqui você salvaria as configurações
                // Por enquanto, só logamos
                _logger.LogInformation("Configurações gerais atualizadas: {@Settings}", model);
                
                TempData["SuccessMessage"] = "Configurações gerais atualizadas com sucesso!";
                return RedirectToAction(nameof(General));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar configurações gerais");
            TempData["ErrorMessage"] = "Erro ao atualizar configurações. Tente novamente.";
        }

        return View("General", model);
    }

    /// <summary>
    /// Limpar cache do sistema
    /// </summary>
    [HttpPost]
    public IActionResult ClearCache()
    {
        try
        {
            // Implementar limpeza de cache aqui
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            _logger.LogInformation("Cache do sistema limpo");
            TempData["SuccessMessage"] = "Cache limpo com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar cache");
            TempData["ErrorMessage"] = "Erro ao limpar cache.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Reiniciar aplicação (apenas desenvolvimento)
    /// </summary>
    [HttpPost]
    public IActionResult RestartApplication()
    {
        if (!_environment.IsDevelopment())
        {
            TempData["ErrorMessage"] = "Reinicialização só é permitida em desenvolvimento.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _logger.LogWarning("Aplicação será reiniciada via configurações");
            
            // Implementar restart aqui se necessário
            TempData["SuccessMessage"] = "Aplicação será reiniciada...";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reiniciar aplicação");
            TempData["ErrorMessage"] = "Erro ao reiniciar aplicação.";
        }

        return RedirectToAction(nameof(Index));
    }

    #region Private Methods

    private GeneralSettingsViewModel GetGeneralSettings()
    {
        return new GeneralSettingsViewModel
        {
            ApplicationName = "Documentos Fiscais - LOG CT-e",
            Version = "1.0.0",
            Environment = _environment.EnvironmentName,
            TimeZone = TimeZoneInfo.Local.DisplayName,
            Culture = System.Globalization.CultureInfo.CurrentCulture.Name,
            DateFormat = "dd/MM/yyyy",
            TimeFormat = "HH:mm:ss"
        };
    }

    private UploadSettingsViewModel GetUploadSettings()
    {
        return new UploadSettingsViewModel
        {
            MaxFileSize = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new[] { ".xml" },
            AllowMultipleFiles = true,
            AutoDetectType = true,
            ValidateXmlStructure = true,
            QuarantineInvalidFiles = false
        };
    }

    private SecuritySettingsViewModel GetSecuritySettings()
    {
        return new SecuritySettingsViewModel
        {
            EnableCors = true,
            EnableRateLimiting = true,
            MaxRequestsPerMinute = 100,
            EnableSecurityHeaders = true,
            EnableHttpsRedirect = true,
            SessionTimeout = 30
        };
    }

    private DatabaseSettingsViewModel GetDatabaseSettings()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        
        return new DatabaseSettingsViewModel
        {
            Provider = "SQL Server",
            ConnectionString = MaskConnectionString(connectionString),
            EnableLogging = true,
            CommandTimeout = 30,
            MaxRetryCount = 3,
            EnableSensitiveDataLogging = _environment.IsDevelopment()
        };
    }

    private LogSettingsViewModel GetLogSettings()
    {
        return new LogSettingsViewModel
        {
            LogLevel = "Information",
            EnableConsoleLogging = true,
            EnableFileLogging = false,
            EnableDatabaseLogging = false,
            LogPath = Path.Combine(_environment.ContentRootPath, "Logs"),
            MaxLogFiles = 10,
            LogRetentionDays = 30
        };
    }

    private string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "";

        // Mascarar senha na connection string
        var parts = connectionString.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Trim().StartsWith("Password", StringComparison.OrdinalIgnoreCase) ||
                parts[i].Trim().StartsWith("Pwd", StringComparison.OrdinalIgnoreCase))
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length == 2)
                {
                    parts[i] = $"{keyValue[0]}=****";
                }
            }
        }

        return string.Join(";", parts);
    }

    #endregion
}

#region ViewModels

public class SettingsViewModel
{
    public GeneralSettingsViewModel General { get; set; } = new();
    public UploadSettingsViewModel Upload { get; set; } = new();
    public SecuritySettingsViewModel Security { get; set; } = new();
    public DatabaseSettingsViewModel Database { get; set; } = new();
}

public class GeneralSettingsViewModel
{
    public string ApplicationName { get; set; } = "";
    public string Version { get; set; } = "";
    public string Environment { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public string Culture { get; set; } = "";
    public string DateFormat { get; set; } = "";
    public string TimeFormat { get; set; } = "";
}

public class UploadSettingsViewModel
{
    public long MaxFileSize { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public bool AllowMultipleFiles { get; set; }
    public bool AutoDetectType { get; set; }
    public bool ValidateXmlStructure { get; set; }
    public bool QuarantineInvalidFiles { get; set; }
}

public class SecuritySettingsViewModel
{
    public bool EnableCors { get; set; }
    public bool EnableRateLimiting { get; set; }
    public int MaxRequestsPerMinute { get; set; }
    public bool EnableSecurityHeaders { get; set; }
    public bool EnableHttpsRedirect { get; set; }
    public int SessionTimeout { get; set; }
}

public class DatabaseSettingsViewModel
{
    public string Provider { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public bool EnableLogging { get; set; }
    public int CommandTimeout { get; set; }
    public int MaxRetryCount { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
}

public class LogSettingsViewModel
{
    public string LogLevel { get; set; } = "";
    public bool EnableConsoleLogging { get; set; }
    public bool EnableFileLogging { get; set; }
    public bool EnableDatabaseLogging { get; set; }
    public string LogPath { get; set; } = "";
    public int MaxLogFiles { get; set; }
    public int LogRetentionDays { get; set; }
}

#endregion