namespace DocumentosFiscais.Core.ViewModels;

public class DashboardViewModel
{
    public int TotalDocumentos { get; set; }
    public int DocumentosHoje { get; set; }
    public int DocumentosPendentes { get; set; }
    public int DocumentosComErro { get; set; }
    
    public decimal ValorTotalMes { get; set; }
    
    public List<DocumentoPorTipoDto> DocumentosPorTipo { get; set; } = [];
    public List<DocumentoPorMesDto> DocumentosPorMes { get; set; } = [];
}

public class DocumentoPorTipoDto
{
    public string Tipo { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string Cor { get; set; } = string.Empty;
}

public class DocumentoPorMesDto
{
    public string Mes { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}