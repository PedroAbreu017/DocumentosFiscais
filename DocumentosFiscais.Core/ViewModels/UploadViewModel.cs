using DocumentosFiscais.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DocumentosFiscais.Core.ViewModels;

public class UploadViewModel
{
    public IFormFile? Arquivo { get; set; }
    public TipoDocumento? TipoSelecionado { get; set; }
    public string? Observacoes { get; set; }
}

public class DocumentoListViewModel
{
    public List<DocumentoFiscal> Documentos { get; set; } = [];
    public int PaginaAtual { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int ItensPorPagina { get; set; } = 10;
    
    // ✅ PROPRIEDADE ADICIONADA - Para contar total de documentos
    public int TotalCount { get; set; }
    
    // ✅ PROPRIEDADE HELPER - Para compatibilidade com os controles de paginação
    public int TotalItens => TotalCount;
    
    // Filtros
    public string? FiltroNome { get; set; }
    public TipoDocumento? FiltroTipo { get; set; }
    public StatusProcessamento? FiltroStatus { get; set; }
    
    // ✅ PROPRIEDADES HELPER - Para melhorar a experiência na view
    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;
    public int PrimeiraPagina => 1;
    public int UltimaPagina => TotalPaginas;
    
    // ✅ PROPRIEDADES PARA INFORMAÇÕES DE PAGINAÇÃO
    public int PrimeiroItem => (PaginaAtual - 1) * ItensPorPagina + 1;
    public int UltimoItem => Math.Min(PaginaAtual * ItensPorPagina, TotalCount);
    
    // ✅ MÉTODO HELPER - Para verificar se tem documentos
    public bool TemDocumentos => Documentos.Count > 0;
    public bool TemFiltrosAtivos => !string.IsNullOrEmpty(FiltroNome) || 
                                   FiltroTipo.HasValue || 
                                   FiltroStatus.HasValue;
}