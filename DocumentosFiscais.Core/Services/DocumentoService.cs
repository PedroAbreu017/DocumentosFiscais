using System.Security.Cryptography;
using System.Text;
using DocumentosFiscais.Core.Models;
using DocumentosFiscais.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using DocumentosFiscais.Core.ViewModels;

namespace DocumentosFiscais.Core.Services;

public class DocumentoService : IDocumentoService
{
    private readonly IDocumentoRepository _repository;
    private readonly IXmlValidationService _xmlService;
    
    public DocumentoService(IDocumentoRepository repository, IXmlValidationService xmlService)
    {
        _repository = repository;
        _xmlService = xmlService;
    }
    
    public async Task<ServiceResult<DocumentoFiscal>> ProcessUploadAsync(IFormFile file, TipoDocumento? tipoForcado = null)
    {
        // Validações básicas
        if (file == null || file.Length == 0)
            return ServiceResult<DocumentoFiscal>.ErrorResult("Arquivo não selecionado ou vazio");
            
        if (!Path.GetExtension(file.FileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<DocumentoFiscal>.ErrorResult("Apenas arquivos XML são permitidos");
            
        if (file.Length > 5 * 1024 * 1024) // 5MB
            return ServiceResult<DocumentoFiscal>.ErrorResult("Arquivo muito grande. Máximo 5MB");
        
        try
        {
            // Ler conteúdo do arquivo
            string xmlContent;
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            xmlContent = await reader.ReadToEndAsync();
            
            // Validar XML
            var validationResult = _xmlService.ValidateXml(xmlContent);
            if (!validationResult.IsValid)
                return ServiceResult<DocumentoFiscal>.ErrorResult($"XML inválido: {validationResult.ErrorMessage}");
            
            // Calcular hash MD5 para evitar duplicatas
            var hashMD5 = CalculateMD5(xmlContent);
            if (await _repository.ExistsAsync(hashMD5))
                return ServiceResult<DocumentoFiscal>.ErrorResult("Este documento já foi importado anteriormente");
            
            // Extrair informações do XML
            var tipo = tipoForcado ?? _xmlService.ExtractDocumentType(xmlContent);
            
            var documento = new DocumentoFiscal
            {
                NomeArquivo = file.FileName,
                Tipo = tipo,
                ConteudoXml = xmlContent,
                TamanhoArquivo = file.Length,
                HashMD5 = hashMD5,
                NumeroDocumento = _xmlService.ExtractDocumentNumber(xmlContent, tipo),
                CnpjEmitente = _xmlService.ExtractEmitterCnpj(xmlContent, tipo),
                NomeEmitente = _xmlService.ExtractEmitterName(xmlContent, tipo),
                ValorTotal = _xmlService.ExtractTotalValue(xmlContent, tipo),
                DataEmissao = _xmlService.ExtractEmissionDate(xmlContent, tipo),
                Status = StatusProcessamento.Processado
            };
            
            var resultado = await _repository.AddAsync(documento);
            var serviceResult = ServiceResult<DocumentoFiscal>.SuccessResult(resultado);
            
            if (validationResult.Warnings.Count != 0)
                serviceResult.Warnings = validationResult.Warnings;
            
            return serviceResult;
        }
        catch (Exception ex)
        {
            return ServiceResult<DocumentoFiscal>.ErrorResult($"Erro ao processar arquivo: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<DocumentoFiscal>> GetByIdAsync(int id)
    {
        try
        {
            var documento = await _repository.GetByIdAsync(id);
            return documento != null 
                ? ServiceResult<DocumentoFiscal>.SuccessResult(documento)
                : ServiceResult<DocumentoFiscal>.ErrorResult("Documento não encontrado");
        }
        catch (Exception ex)
        {
            return ServiceResult<DocumentoFiscal>.ErrorResult($"Erro ao buscar documento: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<PagedResult<DocumentoFiscal>>> GetPagedAsync(int page, int pageSize, string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null)
    {
        try
        {
            var documentos = await _repository.GetPagedAsync(page, pageSize, filtro, tipo, status);
            var totalCount = await _repository.GetTotalCountAsync(filtro, tipo, status);
            
            var result = new PagedResult<DocumentoFiscal>
            {
                Items = documentos.ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            
            return ServiceResult<PagedResult<DocumentoFiscal>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<DocumentoFiscal>>.ErrorResult($"Erro ao buscar documentos: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var resultado = await _repository.DeleteAsync(id);
            return resultado 
                ? ServiceResult<bool>.SuccessResult(true)
                : ServiceResult<bool>.ErrorResult("Documento não encontrado");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Erro ao excluir documento: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<DashboardStats>> GetDashboardStatsAsync()
    {
        try
        {
            var todos = await _repository.GetAllAsync();
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            
            var stats = new DashboardStats
            {
                TotalDocumentos = todos.Count(),
                DocumentosHoje = todos.Count(d => d.DataUpload.Date == hoje),
                DocumentosPendentes = todos.Count(d => d.Status == StatusProcessamento.Pendente),
                DocumentosComErro = todos.Count(d => d.Status == StatusProcessamento.Erro),
                ValorTotalMes = todos.Where(d => d.DataUpload >= inicioMes && d.ValorTotal.HasValue)
                                    .Sum(d => d.ValorTotal!.Value),
                
                DocumentosPorTipo = todos.GroupBy(d => d.Tipo)
                    .Select(g => new DocumentoPorTipoDto 
                    { 
                        Tipo = g.Key.ToString(), 
                        Quantidade = g.Count(),
                        Cor = GetCorPorTipo(g.Key)
                    }).ToList(),
                    
                DocumentosPorMes = todos.Where(d => d.DataUpload >= hoje.AddMonths(-5))
                    .GroupBy(d => new { d.DataUpload.Year, d.DataUpload.Month })
                    .Select(g => new DocumentoPorMesDto
                    {
                        Mes = $"{g.Key.Month:00}/{g.Key.Year}",
                        Quantidade = g.Count()
                    }).OrderBy(x => x.Mes).ToList()
            };
            
            return ServiceResult<DashboardStats>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            return ServiceResult<DashboardStats>.ErrorResult($"Erro ao buscar estatísticas: {ex.Message}");
        }
    }
    
    private static string CalculateMD5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
    
    private static string GetCorPorTipo(TipoDocumento tipo)
    {
        return tipo switch
        {
            TipoDocumento.CTe => "#007bff",
            TipoDocumento.NFe => "#28a745",
            TipoDocumento.MDFe => "#ffc107",
            TipoDocumento.NFCe => "#17a2b8",
            _ => "#6c757d"
        };
    }
}