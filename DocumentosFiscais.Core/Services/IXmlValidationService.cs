using DocumentosFiscais.Core.Models;

namespace DocumentosFiscais.Core.Services;

public interface IXmlValidationService
{
    XmlValidationResult ValidateXml(string xmlContent);
    TipoDocumento ExtractDocumentType(string xmlContent);
    string? ExtractDocumentNumber(string xmlContent, TipoDocumento tipo);
    string? ExtractEmitterCnpj(string xmlContent, TipoDocumento tipo);
    string? ExtractEmitterName(string xmlContent, TipoDocumento tipo);
    decimal? ExtractTotalValue(string xmlContent, TipoDocumento tipo);
    DateTime? ExtractEmissionDate(string xmlContent, TipoDocumento tipo);
}

public class XmlValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = [];
}