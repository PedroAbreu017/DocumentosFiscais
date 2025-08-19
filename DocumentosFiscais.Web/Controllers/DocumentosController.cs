using DocumentosFiscais.Core.Services;
using DocumentosFiscais.Core.Models;
using DocumentosFiscais.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Cors;

namespace DocumentosFiscais.Web.Controllers;

public class DocumentosController : Controller
{
    private readonly IDocumentoService _documentoService;
    private readonly ILogger<DocumentosController> _logger;
    
    public DocumentosController(IDocumentoService documentoService, ILogger<DocumentosController> logger)
    {
        _documentoService = documentoService;
        _logger = logger;
    }
    
    // GET: Documentos
  
public async Task<IActionResult> Index(int page = 1, string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null)
{
    const int pageSize = 10;
    
    var documentosResult = await _documentoService.GetPagedAsync(page, pageSize, filtro, tipo, status);
    
    if (!documentosResult.Success)
    {
        TempData["ErrorMessage"] = documentosResult.ErrorMessage;
        return View(new DocumentoListViewModel());
    }
    
    var pagedResult = documentosResult.Data!;
    
    var viewModel = new DocumentoListViewModel
    {
        Documentos = pagedResult.Items,
        PaginaAtual = page,
        TotalPaginas = pagedResult.TotalPages,
        ItensPorPagina = pageSize,
        TotalCount = pagedResult.TotalCount, // ← ✅ ADICIONE ESTA LINHA
        FiltroNome = filtro,
        FiltroTipo = tipo,
        FiltroStatus = status
    };
    
    ViewBag.TiposDocumento = GetTiposDocumentoSelectList();
    ViewBag.StatusProcessamento = GetStatusProcessamentoSelectList();
    
    return View(viewModel);
}
    
    // GET: Documentos/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var result = await _documentoService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
        
        return View(result.Data);
    }
    
    // GET: Documentos/Upload
    public IActionResult Upload()
    {
        ViewBag.TiposDocumento = GetTiposDocumentoSelectList();
        return View(new UploadViewModel());
    }
    
    // POST: Documentos/Upload - VERSÃO MODERNA COM SEGURANÇA
    [HttpPost]
    [EnableCors("AllowUpload")]
    public async Task<IActionResult> Upload(IFormFile? Arquivo, int? TipoSelecionado)
    {
        try
        {
            _logger.LogInformation("🔄 Upload iniciado - Arquivo: {FileName}, Tamanho: {Size}", 
                Arquivo?.FileName ?? "null", Arquivo?.Length ?? 0);

            // === VALIDAÇÕES DE SEGURANÇA MODERNAS ===
            
            // 1. Validar Origin (CORS)
            var isValidOrigin = ValidateOrigin();
            if (!isValidOrigin)
            {
                _logger.LogWarning("🚫 Origin inválido: {Origin}", Request.Headers.Origin);
                return BadRequest(new { success = false, message = "Origin não autorizado" });
            }

            // 2. Validação de arquivo
            if (Arquivo == null || Arquivo.Length == 0)
            {
                _logger.LogWarning("📁 Arquivo vazio ou nulo");
                return BadRequest(new { success = false, message = "Nenhum arquivo foi enviado" });
            }

            // 3. Validação de tamanho (10MB máximo)
            if (Arquivo.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("📏 Arquivo muito grande: {Size}MB", Arquivo.Length / 1024 / 1024);
                return BadRequest(new { success = false, message = "Arquivo muito grande. Máximo 10MB." });
            }

            // 4. Validação de extensão
            var allowedExtensions = new[] { ".xml", ".XML" };
            var fileExtension = Path.GetExtension(Arquivo.FileName);
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("🚫 Extensão inválida: {Extension}", fileExtension);
                return BadRequest(new { success = false, message = "Apenas arquivos XML são permitidos" });
            }

            // === PROCESSAMENTO DO ARQUIVO ===

            _logger.LogInformation("✅ Validações de segurança aprovadas. Processando arquivo...");

            // Determinar tipo automaticamente se não especificado
            TipoDocumento? tipoDetectado = null;
            if (TipoSelecionado.HasValue && TipoSelecionado.Value >= 0)
            {
                tipoDetectado = (TipoDocumento)TipoSelecionado.Value;
            }

            // Processar upload
            var result = await _documentoService.ProcessUploadAsync(Arquivo, tipoDetectado);
            
            if (result.Success)
            {
                _logger.LogInformation("✅ Upload processado com sucesso. DocumentoID: {Id}", result.Data?.Id);

                // Para upload AJAX/moderno, retornar JSON
                if (Request.Headers.Accept.ToString().Contains("application/json") || 
                    Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = "Documento enviado com sucesso!",
                        documentoId = result.Data?.Id,
                        warnings = result.Warnings,
                        redirectUrl = Url.Action("Details", new { id = result.Data?.Id })
                    });
                }

                // Para upload tradicional, redirecionamento
                TempData["SuccessMessage"] = "Documento enviado com sucesso!";
                
                if (result.Warnings.Count > 0)
                {
                    TempData["WarningMessage"] = string.Join("<br/>", result.Warnings);
                }
                
                return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
            }
            else
            {
                _logger.LogError("❌ Erro no processamento: {Error}", result.ErrorMessage);

                // Para upload AJAX/moderno
                if (Request.Headers.Accept.ToString().Contains("application/json") || 
                    Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = result.ErrorMessage ?? "Erro ao processar arquivo"
                    });
                }

                // Para upload tradicional
                ModelState.AddModelError("", result.ErrorMessage ?? "Erro ao processar arquivo");
                ViewBag.TiposDocumento = GetTiposDocumentoSelectList();
                
                return View(new UploadViewModel { Arquivo = Arquivo });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Erro interno no upload");

            // Para upload AJAX/moderno
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Erro interno do servidor. Tente novamente."
                });
            }

            // Para upload tradicional
            TempData["ErrorMessage"] = "Erro interno. Tente novamente.";
            ViewBag.TiposDocumento = GetTiposDocumentoSelectList();
            return View(new UploadViewModel());
        }
    }

    // === MÉTODOS DE SEGURANÇA ===

    private bool ValidateOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        var referer = Request.Headers.Referer.ToString();
        var host = Request.Host.ToString();

        // Permitir requisições do próprio host
        var allowedOrigins = new[]
        {
            $"http://{host}",
            $"https://{host}",
            "http://localhost:5128",
            "https://localhost:5128"
        };

        // Se não há Origin, verificar Referer
        if (string.IsNullOrEmpty(origin))
        {
            if (string.IsNullOrEmpty(referer))
                return false; // Requisição suspeita sem Origin nem Referer

            return allowedOrigins.Any(allowed => referer.StartsWith(allowed));
        }

        return allowedOrigins.Contains(origin);
    }

    private string GetClientIP()
    {
        var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIP = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIP))
        {
            return xRealIP;
        }

        return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
    
    // POST: Documentos/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _documentoService.DeleteAsync(id);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Documento excluído com sucesso!";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    // GET: Documentos/Download/5
    public async Task<IActionResult> Download(int id)
    {
        var result = await _documentoService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
        
        var documento = result.Data!;
        var bytes = System.Text.Encoding.UTF8.GetBytes(documento.ConteudoXml);
        
        return File(bytes, "application/xml", documento.NomeArquivo);
    }
    
    // GET: Documentos/ViewXml/5
    public async Task<IActionResult> ViewXml(int id)
    {
        var result = await _documentoService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound();
        }
        
        ViewBag.XmlContent = result.Data!.ConteudoXml;
        ViewBag.NomeArquivo = result.Data.NomeArquivo;
        
        return View();
    }
    
    private static IEnumerable<SelectListItem> GetTiposDocumentoSelectList()
    {
        return Enum.GetValues<TipoDocumento>()
            .Select(t => new SelectListItem 
            { 
                Value = ((int)t).ToString(), 
                Text = t.ToString() 
            });
    }
    
    private static IEnumerable<SelectListItem> GetStatusProcessamentoSelectList()
    {
        return Enum.GetValues<StatusProcessamento>()
            .Select(s => new SelectListItem 
            { 
                Value = ((int)s).ToString(), 
                Text = GetStatusText(s) 
            });
    }
    
    private static string GetStatusText(StatusProcessamento status)
    {
        return status switch
        {
            StatusProcessamento.Pendente => "Pendente",
            StatusProcessamento.Processando => "Processando",
            StatusProcessamento.Processado => "Processado",
            StatusProcessamento.Erro => "Erro",
            StatusProcessamento.Cancelado => "Cancelado",
            _ => status.ToString()
        };
    }
}