using Microsoft.AspNetCore.Mvc;

namespace DocumentosFiscais.Web.Controllers
{
    public class HelpController : Controller
    {
        private readonly ILogger<HelpController> _logger;

        public HelpController(ILogger<HelpController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Central de Ajuda";
            return View();
        }

        public IActionResult FAQ()
        {
            ViewData["Title"] = "Perguntas Frequentes";
            return View();
        }

        public IActionResult Documentation()
        {
            ViewData["Title"] = "Documentação";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contato";
            return View();
        }

        public IActionResult SystemInfo()
        {
            ViewData["Title"] = "Informações do Sistema";

            try
            {
                var systemInfo = new
                {
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    Framework = ".NET 8.0",
                    Database = "SQL Server Express",
                    ServerTime = DateTime.Now,
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = FormatBytes(Environment.WorkingSet),
                    TotalMemory = FormatBytes(GC.GetTotalMemory(false)),
                    Uptime = GetUptime(),
                    SupportedFormats = new[] { "CT-e", "NF-e", "MDF-e", "NFC-e" },
                    MaxFileSize = "10 MB",
                    SupportedFileTypes = new[] { ".xml" }
                };

                return View(systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do sistema");
                
                // Fallback com dados básicos
                var basicInfo = new
                {
                    Version = "1.0.0",
                    Environment = "Unknown",
                    Framework = ".NET 8.0",
                    Database = "SQL Server Express",
                    ServerTime = DateTime.Now,
                    Error = "Algumas informações não puderam ser obtidas"
                };

                return View(basicInfo);
            }
        }

        [HttpPost]
        public IActionResult SendFeedback([FromBody] FeedbackModel feedback)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(feedback.Name) || 
                    string.IsNullOrWhiteSpace(feedback.Email) || 
                    string.IsNullOrWhiteSpace(feedback.Message))
                {
                    return Json(new { success = false, message = "Preencha todos os campos obrigatórios." });
                }

                // Log do feedback recebido
                _logger.LogInformation("Feedback recebido de {Name} ({Email}): {Subject}", 
                    feedback.Name, feedback.Email, feedback.Subject);

                // Aqui você implementaria o envio real do feedback
                // Por exemplo: salvar no banco, enviar email, etc.
                
                // Simulação de processamento
                ProcessFeedback(feedback);

                return Json(new { success = true, message = "Feedback enviado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar feedback de {Email}", feedback?.Email);
                return Json(new { success = false, message = "Erro ao enviar feedback. Tente novamente." });
            }
        }

        // Métodos auxiliares
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetUptime()
        {
            try
            {
                var startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
                var uptime = DateTime.Now - startTime;
                
                if (uptime.Days > 0)
                    return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
                else if (uptime.Hours > 0)
                    return $"{uptime.Hours}h {uptime.Minutes}m";
                else
                    return $"{uptime.Minutes}m {uptime.Seconds}s";
            }
            catch
            {
                return "N/A";
            }
        }

        private void ProcessFeedback(FeedbackModel feedback)
        {
            // Implementar processamento real do feedback aqui
            // Exemplos:
            // - Salvar no banco de dados
            // - Enviar email para suporte
            // - Integrar com sistema de tickets
            // - Notificar administradores
            
            _logger.LogInformation("Processando feedback: {Type} - Rating: {Rating}", 
                feedback.Type, feedback.Rating);
        }
    }

    public class FeedbackModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "general";
        public int Rating { get; set; } = 5;
    }
}