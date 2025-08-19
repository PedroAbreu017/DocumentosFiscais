// DocumentosFiscais.Core/Models/DocumentoFiscal.cs
using System.ComponentModel.DataAnnotations;

namespace DocumentosFiscais.Core.Models;

public class DocumentoFiscal
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Nome do arquivo é obrigatório")]
    [StringLength(255, ErrorMessage = "Nome do arquivo deve ter no máximo 255 caracteres")]
    public string NomeArquivo { get; set; } = string.Empty;
    
    [Required]
    public TipoDocumento Tipo { get; set; }
    
    [Required(ErrorMessage = "Conteúdo XML é obrigatório")]
    public string ConteudoXml { get; set; } = string.Empty;
    
    public DateTime DataUpload { get; set; } = DateTime.UtcNow;
    
    public StatusProcessamento Status { get; set; } = StatusProcessamento.Pendente;
    
    [StringLength(50)]
    public string? NumeroDocumento { get; set; }
    
    public long TamanhoArquivo { get; set; }
    
    [StringLength(100)]
    public string? HashMD5 { get; set; }
    
    // Dados extraídos do XML
    [StringLength(14)]
    public string? CnpjEmitente { get; set; }
    
    [StringLength(255)]
    public string? NomeEmitente { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal? ValorTotal { get; set; }
    
    public DateTime? DataEmissao { get; set; }
}
