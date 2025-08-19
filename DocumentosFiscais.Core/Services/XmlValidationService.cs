using System.Xml;
using System.Xml.Linq;
using DocumentosFiscais.Core.Models;

namespace DocumentosFiscais.Core.Services;

public class XmlValidationService : IXmlValidationService
{
    public XmlValidationResult ValidateXml(string xmlContent)
    {
        var result = new XmlValidationResult();
        
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            result.IsValid = false;
            result.ErrorMessage = "Conteúdo XML não pode ser vazio";
            return result;
        }
        
        try
        {
            XDocument.Parse(xmlContent);
            result.IsValid = true;
        }
        catch (XmlException ex)
        {
            result.IsValid = false;
            result.ErrorMessage = $"XML inválido: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Erro ao validar XML: {ex.Message}";
        }
        
        return result;
    }
    
    public TipoDocumento ExtractDocumentType(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var rootElement = doc.Root?.Name.LocalName.ToUpper();
            
            return rootElement switch
            {
                "CTE" => TipoDocumento.CTe,
                "NFE" => TipoDocumento.NFe,
                "MDFE" => TipoDocumento.MDFe,
                "NFCE" => TipoDocumento.NFCe,
                _ => TipoDocumento.Outros
            };
        }
        catch
        {
            return TipoDocumento.Outros;
        }
    }
    
    public string? ExtractDocumentNumber(string xmlContent, TipoDocumento tipo)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            return tipo switch
            {
                TipoDocumento.CTe => doc.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "nCT")?.Value,
                TipoDocumento.NFe => doc.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "nNF")?.Value,
                TipoDocumento.MDFe => doc.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "nMDF")?.Value,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
    
    public string? ExtractEmitterCnpj(string xmlContent, TipoDocumento tipo)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            return doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "emit")?
                .Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "CNPJ")?.Value;
        }
        catch
        {
            return null;
        }
    }
    
    public string? ExtractEmitterName(string xmlContent, TipoDocumento tipo)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            return doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "emit")?
                .Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "xNome")?.Value;
        }
        catch
        {
            return null;
        }
    }
    
    public decimal? ExtractTotalValue(string xmlContent, TipoDocumento tipo)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            var valueElement = tipo switch
            {
                TipoDocumento.CTe => doc.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "vTPrest")?.Value,
                TipoDocumento.NFe => doc.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "vNF")?.Value,
                _ => null
            };
            
            return decimal.TryParse(valueElement, out var value) ? value : null;
        }
        catch
        {
            return null;
        }
    }
    
    public DateTime? ExtractEmissionDate(string xmlContent, TipoDocumento tipo)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            var dateElement = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "dhEmi")?.Value;
            
            return DateTime.TryParse(dateElement, out var date) ? date : null;
        }
        catch
        {
            return null;
        }
    }
}

