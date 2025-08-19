using DocumentosFiscais.Core.Models;

namespace DocumentosFiscais.Core.Interfaces;

public interface IDocumentoRepository
{
    Task<IEnumerable<DocumentoFiscal>> GetAllAsync();
    Task<DocumentoFiscal?> GetByIdAsync(int id);
    Task<DocumentoFiscal> AddAsync(DocumentoFiscal documento);
    Task<DocumentoFiscal> UpdateAsync(DocumentoFiscal documento);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<DocumentoFiscal>> GetPagedAsync(int page, int pageSize, string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null);
    Task<int> GetTotalCountAsync(string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null);
    Task<bool> ExistsAsync(string hashMD5);
}