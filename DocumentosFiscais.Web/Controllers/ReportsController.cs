using DocumentosFiscais.Core.Models;
using DocumentosFiscais.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DocumentosFiscais.Web.Controllers;

/// <summary>
/// Controller para relatórios e análises
/// </summary>
public class ReportsController : Controller
{
    private readonly IDocumentoService _documentoService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IDocumentoService documentoService, ILogger<ReportsController> logger)
    {
        _documentoService = documentoService;
        _logger = logger;
    }

    /// <summary>
    /// Página principal de relatórios
    /// </summary>
    public IActionResult Index()
    {
        ViewBag.Title = "Relatórios";
        return View();
    }

    /// <summary>
    /// Relatório de documentos por período
    /// </summary>
    public async Task<IActionResult> DocumentsByPeriod(DateTime? startDate = null, DateTime? endDate = null, TipoDocumento? tipo = null)
    {
        ViewBag.Title = "Documentos por Período";
        
        // Valores padrão
        startDate ??= DateTime.Today.AddDays(-30);
        endDate ??= DateTime.Today;

        try
        {
            // Buscar documentos do período
            var result = await _documentoService.GetPagedAsync(1, 1000, null, tipo, null);
            
            if (result.Success && result.Data != null)
            {
                var documentos = result.Data.Items
                    .Where(d => d.DataUpload >= startDate && d.DataUpload <= endDate)
                    .ToList();

                var viewModel = new DocumentsByPeriodViewModel
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TipoFiltro = tipo,
                    Documentos = documentos,
                    TotalDocumentos = documentos.Count,
                    TotalValor = documentos.Sum(d => d.ValorTotal ?? 0),
                    DocumentosPorTipo = documentos
                        .GroupBy(d => d.Tipo)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    DocumentosPorStatus = documentos
                        .GroupBy(d => d.Status)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                ViewBag.TiposDocumento = GetTiposDocumentoSelectList();
                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de documentos por período");
            TempData["ErrorMessage"] = "Erro ao gerar relatório. Tente novamente.";
        }

        // Em caso de erro, retornar view vazia
        var emptyViewModel = new DocumentsByPeriodViewModel
        {
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            TipoFiltro = tipo,
            Documentos = new List<DocumentoFiscal>()
        };

        ViewBag.TiposDocumento = GetTiposDocumentoSelectList();
        return View(emptyViewModel);
    }

    /// <summary>
    /// Relatório de performance do sistema
    /// </summary>
    public async Task<IActionResult> Performance()
    {
        ViewBag.Title = "Performance do Sistema";

        try
        {
            var result = await _documentoService.GetPagedAsync(1, 100, null, null, null);
            
            if (result.Success && result.Data != null)
            {
                var documentos = result.Data.Items;
                
                var viewModel = new PerformanceReportViewModel
                {
                    TotalDocumentos = documentos.Count,
                    DocumentosProcessados = documentos.Count(d => d.Status == StatusProcessamento.Processado),
                    DocumentosPendentes = documentos.Count(d => d.Status == StatusProcessamento.Pendente),
                    DocumentosComErro = documentos.Count(d => d.Status == StatusProcessamento.Erro),
                    TamanhoMedioArquivo = documentos.Any() ? documentos.Average(d => (double)d.TamanhoArquivo) : 0,
                    UltimasAtividades = documentos
                        .OrderByDescending(d => d.DataUpload)
                        .Take(10)
                        .ToList()
                };

                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de performance");
            TempData["ErrorMessage"] = "Erro ao gerar relatório de performance.";
        }

        return View(new PerformanceReportViewModel());
    }

    /// <summary>
    /// Exportar relatório para CSV
    /// </summary>
    public async Task<IActionResult> ExportToCsv(DateTime? startDate = null, DateTime? endDate = null, TipoDocumento? tipo = null)
    {
        try
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var result = await _documentoService.GetPagedAsync(1, 10000, null, tipo, null);
            
            if (result.Success && result.Data != null)
            {
                var documentos = result.Data.Items
                    .Where(d => d.DataUpload >= startDate && d.DataUpload <= endDate)
                    .ToList();

                var csv = GenerateCsv(documentos);
                var fileName = $"documentos_fiscais_{startDate:yyyyMMdd}_a_{endDate:yyyyMMdd}.csv";

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar relatório para CSV");
            TempData["ErrorMessage"] = "Erro ao exportar relatório.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Gera conteúdo CSV dos documentos
    /// </summary>
    private string GenerateCsv(List<DocumentoFiscal> documentos)
    {
        var csv = new System.Text.StringBuilder();
        
        // Cabeçalho
        csv.AppendLine("ID,Numero,Tipo,Emitente,CNPJ,Data Emissao,Data Upload,Status,Valor Total,Tamanho Arquivo");

        // Dados
        foreach (var doc in documentos)
        {
            csv.AppendLine($"{doc.Id}," +
                          $"\"{doc.NumeroDocumento}\"," +
                          $"{doc.Tipo}," +
                          $"\"{doc.NomeEmitente}\"," +
                          $"\"{doc.CnpjEmitente}\"," +
                          $"{doc.DataEmissao:yyyy-MM-dd}," +
                          $"{doc.DataUpload:yyyy-MM-dd HH:mm:ss}," +
                          $"{doc.Status}," +
                          $"{doc.ValorTotal:F2}," +
                          $"{doc.TamanhoArquivo}");
        }

        return csv.ToString();
    }

    /// <summary>
    /// Obtém lista de tipos de documento para select
    /// </summary>
    private IEnumerable<SelectListItem> GetTiposDocumentoSelectList()
    {
        var items = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todos os tipos" }
        };

        items.AddRange(Enum.GetValues<TipoDocumento>()
            .Select(t => new SelectListItem 
            { 
                Value = ((int)t).ToString(), 
                Text = t.ToString() 
            }));

        return items;
    }
}

/// <summary>
/// ViewModel para relatório de documentos por período
/// </summary>
public class DocumentsByPeriodViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TipoDocumento? TipoFiltro { get; set; }
    public List<DocumentoFiscal> Documentos { get; set; } = new();
    public int TotalDocumentos { get; set; }
    public decimal TotalValor { get; set; }
    public Dictionary<TipoDocumento, int> DocumentosPorTipo { get; set; } = new();
    public Dictionary<StatusProcessamento, int> DocumentosPorStatus { get; set; } = new();
}

/// <summary>
/// ViewModel para relatório de performance
/// </summary>
public class PerformanceReportViewModel
{
    public int TotalDocumentos { get; set; }
    public int DocumentosProcessados { get; set; }
    public int DocumentosPendentes { get; set; }
    public int DocumentosComErro { get; set; }
    public double TamanhoMedioArquivo { get; set; } // MUDOU: long -> double
    public List<DocumentoFiscal> UltimasAtividades { get; set; } = new();
    
    public double TaxaSucesso => TotalDocumentos > 0 ? 
        (double)DocumentosProcessados / TotalDocumentos * 100 : 0;
}