using DocumentosFiscais.Core.Models;
using DocumentosFiscais.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentosFiscais.Web.Controllers.Api
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IDocumentoService _documentoService;

        public DashboardApiController(IDocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        /// <summary>
        /// Retorna estatísticas completas do dashboard - TIPOS CORRIGIDOS
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetStats()
        {
            try
            {
                var result = await _documentoService.GetDashboardStatsAsync();

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.ErrorMessage ?? "Erro ao obter estatísticas"
                    });
                }

                // ✅ TIPOS CORRIGIDOS - usar Cast<object>() para converter
                var documentsByType = result.Data?.DocumentosPorTipo?.Select(d => new {
                    tipo = d.Tipo,
                    quantidade = d.Quantidade,
                    color = d.Cor
                }).Cast<object>().ToList() ?? new List<object>();

                var monthlyEvolution = result.Data?.DocumentosPorMes?.Select(d => new {
                    mes = d.Mes,
                    documentos = d.Quantidade
                }).Cast<object>().ToList() ?? new List<object>();

                var expandedData = new
                {
                    // Dados básicos do dashboard - REAIS
                    totalDocuments = result.Data?.TotalDocumentos ?? 0,
                    documentsToday = result.Data?.DocumentosHoje ?? 0,
                    pendingDocuments = result.Data?.DocumentosPendentes ?? 0,
                    errorDocuments = result.Data?.DocumentosComErro ?? 0,
                    totalValue = result.Data?.ValorTotalMes ?? 0,
                    
                    // Dados para gráficos - REAIS com tipos corretos
                    documentsByType = documentsByType,
                    documentsByStatus = GetDocumentsByStatusData(result.Data),
                    monthlyEvolution = monthlyEvolution,
                    recentActivity = await GetRecentActivityDataReal()
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = expandedData,
                    Message = "Estatísticas recuperadas com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro interno: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Retorna resumo rápido para widgets
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<DashboardSummary>>> GetSummary()
        {
            try
            {
                var result = await _documentoService.GetDashboardStatsAsync();

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<DashboardSummary>
                    {
                        Success = false,
                        Message = result.ErrorMessage ?? "Erro ao obter resumo"
                    });
                }

                var summary = new DashboardSummary
                {
                    TotalDocumentos = result.Data?.TotalDocumentos ?? 0,
                    DocumentosHoje = result.Data?.DocumentosHoje ?? 0,
                    DocumentosPendentes = result.Data?.DocumentosPendentes ?? 0,
                    DocumentosComErro = result.Data?.DocumentosComErro ?? 0,
                    ValorTotalMes = result.Data?.ValorTotalMes ?? 0
                };

                return Ok(new ApiResponse<DashboardSummary>
                {
                    Success = true,
                    Data = summary,
                    Message = "Resumo recuperado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DashboardSummary>
                {
                    Success = false,
                    Message = $"Erro interno: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Retorna dados para gráficos - TIPOS CORRIGIDOS
        /// </summary>
        [HttpGet("charts")]
        public async Task<ActionResult<ApiResponse<object>>> GetChartData()
        {
            try
            {
                var statsResult = await _documentoService.GetDashboardStatsAsync();

                if (!statsResult.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = statsResult.ErrorMessage ?? "Erro ao obter dados dos gráficos"
                    });
                }

                var chartData = new
                {
                    pieChart = statsResult.Data?.DocumentosPorTipo?.Select(d => new {
                        tipo = d.Tipo,
                        quantidade = d.Quantidade,
                        color = d.Cor
                    }).Cast<object>().ToList() ?? new List<object>(),
                    
                    lineChart = statsResult.Data?.DocumentosPorMes?.Select(d => new {
                        mes = d.Mes,
                        documentos = d.Quantidade
                    }).Cast<object>().ToList() ?? new List<object>(),
                    
                    barChart = GetDocumentsByStatusData(statsResult.Data)
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = chartData,
                    Message = "Dados dos gráficos recuperados com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao obter dados dos gráficos: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Retorna atividade recente - CORRIGIDO
        /// </summary>
        [HttpGet("activity")]
        public async Task<ActionResult<ApiResponse<object>>> GetActivity()
        {
            try
            {
                var activity = await GetRecentActivityDataReal();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = activity,
                    Message = "Atividade recente recuperada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao obter atividade recente: {ex.Message}"
                });
            }
        }

        // ✅ MÉTODOS AUXILIARES CORRIGIDOS

        private List<object> GetDocumentsByStatusData(DashboardStats? stats)
        {
            if (stats != null)
            {
                var processados = stats.TotalDocumentos - stats.DocumentosPendentes - stats.DocumentosComErro;
                
                return new List<object>
                {
                    new { status = "Processados", quantidade = processados, color = "#10b981" },
                    new { status = "Pendentes", quantidade = stats.DocumentosPendentes, color = "#f59e0b" },
                    new { status = "Com Erro", quantidade = stats.DocumentosComErro, color = "#ef4444" }
                };
            }
            
            // Fallback apenas se não houver dados
            return new List<object>
            {
                new { status = "Processados", quantidade = 0, color = "#10b981" },
                new { status = "Pendentes", quantidade = 0, color = "#f59e0b" },
                new { status = "Com Erro", quantidade = 0, color = "#ef4444" }
            };
        }

        /// <summary>
        /// Busca atividade recente REAL do banco de dados
        /// </summary>
        private async Task<List<object>> GetRecentActivityDataReal()
        {
            try
            {
                // Buscar últimos 5 documentos reais
                var recentDocsResult = await _documentoService.GetPagedAsync(1, 5);
                
                if (recentDocsResult.Success && recentDocsResult.Data != null)
                {
                    return recentDocsResult.Data.Items.Select(doc => new
                    {
                        tipo = GetTipoDisplayName(doc.Tipo),
                        nomeArquivo = doc.NomeArquivo,
                        tempoRelativo = GetTempoRelativo(doc.DataUpload),
                        icone = GetTipoIcon(doc.Tipo),
                        cor = GetTipoColor(doc.Tipo)
                    }).Cast<object>().ToList();
                }
            }
            catch (Exception ex)
            {
                // Log do erro, mas não quebrar a API
                Console.WriteLine($"Erro ao buscar atividade recente: {ex.Message}");
            }
            
            // Fallback vazio se houver erro
            return new List<object>();
        }

        // Métodos utilitários
        private string GetTipoDisplayName(TipoDocumento tipo)
        {
            return tipo switch
            {
                TipoDocumento.CTe => "CT-e",
                TipoDocumento.NFe => "NF-e",
                TipoDocumento.MDFe => "MDF-e",
                TipoDocumento.NFCe => "NFC-e",
                _ => tipo.ToString()
            };
        }

        private string GetTipoColor(TipoDocumento tipo)
        {
            return tipo switch
            {
                TipoDocumento.CTe => "#6366f1",
                TipoDocumento.NFe => "#10b981",
                TipoDocumento.MDFe => "#f59e0b",
                TipoDocumento.NFCe => "#ec4899",
                _ => "#6b7280"
            };
        }

        private string GetTipoIcon(TipoDocumento tipo)
        {
            return tipo switch
            {
                TipoDocumento.CTe => "bi-truck",
                TipoDocumento.NFe => "bi-receipt",
                TipoDocumento.MDFe => "bi-journals",
                TipoDocumento.NFCe => "bi-receipt-cutoff",
                _ => "bi-file-text"
            };
        }

        private string GetTempoRelativo(DateTime dataUpload)
        {
            var agora = DateTime.Now;
            var diferenca = agora - dataUpload;

            if (diferenca.TotalMinutes < 1)
                return "Agora mesmo";
            if (diferenca.TotalMinutes < 60)
                return $"{(int)diferenca.TotalMinutes} min atrás";
            if (diferenca.TotalHours < 24)
                return $"{(int)diferenca.TotalHours}h atrás";
            if (diferenca.TotalDays < 7)
                return $"{(int)diferenca.TotalDays} dias atrás";
            
            return dataUpload.ToString("dd/MM/yyyy");
        }
    }

    public class DashboardSummary
    {
        public int TotalDocumentos { get; set; }
        public int DocumentosHoje { get; set; }
        public int DocumentosPendentes { get; set; }
        public int DocumentosComErro { get; set; }
        public decimal ValorTotalMes { get; set; }
    }
}