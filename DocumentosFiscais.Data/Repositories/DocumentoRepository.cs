using DocumentosFiscais.Core.Models;
using Microsoft.EntityFrameworkCore;
using DocumentosFiscais.Core.Interfaces;

namespace DocumentosFiscais.Data.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly DocumentosContext _context;
    
    public DocumentoRepository(DocumentosContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<DocumentoFiscal>> GetAllAsync()
    {
        return await _context.DocumentosFiscais
            .OrderByDescending(d => d.DataUpload)
            .ToListAsync();
    }
    
    public async Task<DocumentoFiscal?> GetByIdAsync(int id)
    {
        return await _context.DocumentosFiscais.FindAsync(id);
    }
    
    public async Task<DocumentoFiscal> AddAsync(DocumentoFiscal documento)
    {
        _context.DocumentosFiscais.Add(documento);
        await _context.SaveChangesAsync();
        return documento;
    }
    
    public async Task<DocumentoFiscal> UpdateAsync(DocumentoFiscal documento)
    {
        _context.Entry(documento).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return documento;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var documento = await GetByIdAsync(id);
        if (documento == null)
            return false;
            
        _context.DocumentosFiscais.Remove(documento);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<DocumentoFiscal>> GetPagedAsync(int page, int pageSize, string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null)
    {
        var query = _context.DocumentosFiscais.AsQueryable();
        
        // Aplicar filtros
        if (!string.IsNullOrEmpty(filtro))
            query = query.Where(d => d.NomeArquivo.Contains(filtro) ||
                                    (d.NumeroDocumento != null && d.NumeroDocumento.Contains(filtro)));
                                    
        if (tipo.HasValue)
            query = query.Where(d => d.Tipo == tipo.Value);
            
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        
        return await query
            .OrderByDescending(d => d.DataUpload)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalCountAsync(string? filtro = null, TipoDocumento? tipo = null, StatusProcessamento? status = null)
    {
        var query = _context.DocumentosFiscais.AsQueryable();
        
        // Aplicar os mesmos filtros
        if (!string.IsNullOrEmpty(filtro))
            query = query.Where(d => d.NomeArquivo.Contains(filtro) ||
                                    (d.NumeroDocumento != null && d.NumeroDocumento.Contains(filtro)));
                                    
        if (tipo.HasValue)
            query = query.Where(d => d.Tipo == tipo.Value);
            
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        
        return await query.CountAsync();
    }
    
    public async Task<bool> ExistsAsync(string hashMD5)
    {
        return await _context.DocumentosFiscais
            .AnyAsync(d => d.HashMD5 == hashMD5);
    }
}