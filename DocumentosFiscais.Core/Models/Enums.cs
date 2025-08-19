
namespace DocumentosFiscais.Core.Models;

public enum TipoDocumento
{
    CTe = 1,
    NFe = 2,
    MDFe = 3,
    NFCe = 4,
    Outros = 99
}

public enum StatusProcessamento
{
    Pendente = 1,
    Processando = 2,
    Processado = 3,
    Erro = 4,
    Cancelado = 5
}
