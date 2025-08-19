using DocumentosFiscais.Core.Models;
using DocumentosFiscais.Core.ViewModels;
using Microsoft.AspNetCore.Http;

namespace DocumentosFiscais.Core.Services;

public interface IDocumentoService
{
    Task<ServiceResult<DocumentoFiscal>> ProcessUploadAsync(IFormFile file, TipoDocumento? tipoForcado = null);
    Task<ServiceResult<DocumentoFiscal>> GetByIdAsync(int id);
    Task<ServiceResult<PagedResult<DocumentoFiscal>>> GetPagedAsync(int page, int pageSize, string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    Task<ServiceResult<DashboardStats>> GetDashboardStatsAsync();
}

public class DashboardStats
{
    public int TotalDocumentos { get; set; }
    public int DocumentosHoje { get; set; }
    public int DocumentosPendentes { get; set; }
    public int DocumentosComErro { get; set; }
    public decimal ValorTotalMes { get; set; }
    public List<DocumentoPorTipoDto> DocumentosPorTipo { get; set; } = [];
    public List<DocumentoPorMesDto> DocumentosPorMes { get; set; } = [];
}
