using DocumentosFiscais.Core.Models;
using DocumentosFiscais.Data;
using Microsoft.Extensions.Logging;

namespace DocumentosFiscais.Data.Seed
{
    public class SeedDataService
    {
        private readonly DocumentosContext _context;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(DocumentosContext context, ILogger<SeedDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Verifica se já existem dados
                if (_context.DocumentosFiscais.Any())
                {
                    _logger.LogInformation("⚠️ Dados já existem no banco - Seed cancelado");
                    return;
                }

                _logger.LogInformation("🌱 Iniciando criação de dados de exemplo...");

                var documentos = CreateSampleDocuments();

                // Adicionar ao contexto
                _context.DocumentosFiscais.AddRange(documentos);
                await _context.SaveChangesAsync();

                LogSeedResults(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro durante o seed de dados");
                throw;
            }
        }

        private List<DocumentoFiscal> CreateSampleDocuments()
        {
            var documentos = new List<DocumentoFiscal>();
            var random = new Random();

            // CT-e - Conhecimento de Transporte
            documentos.AddRange(CreateCTeDocuments(random));

            // NF-e - Nota Fiscal Eletrônica  
            documentos.AddRange(CreateNFeDocuments(random));

            // MDF-e - Manifesto de Documentos Fiscais
            documentos.AddRange(CreateMDFeDocuments(random));

            // Documentos históricos para gráficos
            documentos.AddRange(CreateHistoricalDocuments(random));

            return documentos;
        }

        private List<DocumentoFiscal> CreateCTeDocuments(Random random)
        {
            var documentos = new List<DocumentoFiscal>();

            for (int i = 1; i <= 5; i++)
            {
                documentos.Add(new DocumentoFiscal
                {
                    NomeArquivo = $"35220114200166000187570010000{i:D6}1871234{i:D2}.xml",
                    TamanhoArquivo = random.Next(50000, 200000),
                    HashMD5 = Guid.NewGuid().ToString("N")[..32],
                    Tipo = TipoDocumento.CTe,
                    Status = i <= 4 ? StatusProcessamento.Processado : StatusProcessamento.Pendente,
                    DataUpload = DateTime.Now.AddDays(-random.Next(0, 30)),
                    DataEmissao = DateTime.Now.AddDays(-random.Next(1, 45)),
                    NumeroDocumento = $"00000{i:D6}",
                    NomeEmitente = GetRandomTransportadora(random),
                    CnpjEmitente = "14.200.166/0001-87",
                    ValorTotal = (decimal)(random.NextDouble() * 5000 + 1000),
                    ConteudoXml = CreateCTeXml(i)
                });
            }

            return documentos;
        }

        private List<DocumentoFiscal> CreateNFeDocuments(Random random)
        {
            var documentos = new List<DocumentoFiscal>();

            for (int i = 1; i <= 3; i++)
            {
                documentos.Add(new DocumentoFiscal
                {
                    NomeArquivo = $"35220207850000000{i:D3}550010000{i:D6}1123456{i:D2}.xml",
                    TamanhoArquivo = random.Next(80000, 300000),
                    HashMD5 = Guid.NewGuid().ToString("N")[..32],
                    Tipo = TipoDocumento.NFe,
                    Status = StatusProcessamento.Processado,
                    DataUpload = DateTime.Now.AddDays(-random.Next(0, 15)),
                    DataEmissao = DateTime.Now.AddDays(-random.Next(1, 20)),
                    NumeroDocumento = $"00000{i:D6}",
                    NomeEmitente = GetRandomEmpresa(random),
                    CnpjEmitente = "07.850.000/0001-23",
                    ValorTotal = (decimal)(random.NextDouble() * 15000 + 2000),
                    ConteudoXml = CreateNFeXml(i)
                });
            }

            return documentos;
        }

        private List<DocumentoFiscal> CreateMDFeDocuments(Random random)
        {
            var documentos = new List<DocumentoFiscal>();

            for (int i = 1; i <= 2; i++)
            {
                documentos.Add(new DocumentoFiscal
                {
                    NomeArquivo = $"35220214200166000187580010000{i:D6}1871234{i:D2}.xml",
                    TamanhoArquivo = random.Next(30000, 100000),
                    HashMD5 = Guid.NewGuid().ToString("N")[..32],
                    Tipo = TipoDocumento.MDFe,
                    Status = StatusProcessamento.Processado,
                    DataUpload = DateTime.Now.AddDays(-random.Next(0, 10)),
                    DataEmissao = DateTime.Now.AddDays(-random.Next(1, 15)),
                    NumeroDocumento = $"00000{i:D6}",
                    NomeEmitente = GetRandomTransportadora(random),
                    CnpjEmitente = "14.200.166/0001-87",
                    ValorTotal = (decimal)(random.NextDouble() * 8000 + 3000),
                    ConteudoXml = CreateMDFeXml(i)
                });
            }

            return documentos;
        }

        private List<DocumentoFiscal> CreateHistoricalDocuments(Random random)
        {
            var documentos = new List<DocumentoFiscal>();

            // Criar documentos dos últimos 6 meses para gráficos
            for (int mes = 1; mes <= 6; mes++)
            {
                int quantidade = random.Next(3, 8);
                for (int i = 1; i <= quantidade; i++)
                {
                    var tipos = new[] { TipoDocumento.CTe, TipoDocumento.NFe, TipoDocumento.MDFe };
                    var tipo = tipos[random.Next(tipos.Length)];
                    
                    documentos.Add(new DocumentoFiscal
                    {
                        NomeArquivo = $"historico-{mes:D2}-{i:D3}-{Guid.NewGuid().ToString("N")[..8]}.xml",
                        TamanhoArquivo = random.Next(40000, 150000),
                        HashMD5 = Guid.NewGuid().ToString("N")[..32],
                        Tipo = tipo,
                        Status = random.Next(10) > 1 ? StatusProcessamento.Processado : StatusProcessamento.Pendente,
                        DataUpload = DateTime.Now.AddMonths(-mes).AddDays(random.Next(0, 28)),
                        DataEmissao = DateTime.Now.AddMonths(-mes).AddDays(-random.Next(1, 10)),
                        NumeroDocumento = $"{mes:D2}{i:D4}",
                        NomeEmitente = tipo == TipoDocumento.CTe 
                            ? GetRandomTransportadora(random) 
                            : GetRandomEmpresa(random),
                        CnpjEmitente = $"12.345.{mes:D3}/0001-{i:D2}",
                        ValorTotal = (decimal)(random.NextDouble() * 10000 + 500),
                        ConteudoXml = CreateSimpleXml(tipo.ToString(), i)
                    });
                }
            }

            return documentos;
        }

        // Métodos auxiliares internos
        private string GetRandomTransportadora(Random random)
        {
            var transportadoras = new[]
            {
                "LOG CT-e Transportes Ltda",
                "TransLog Soluções Logísticas",
                "RodoLog Transportes SA",
                "ViaLog Logística Integrada",
                "CargoLog Transportes"
            };
            return transportadoras[random.Next(transportadoras.Length)];
        }

        private string GetRandomEmpresa(Random random)
        {
            var empresas = new[]
            {
                "TechSolutions Informática Ltda",
                "InnovaCorp Tecnologia SA",
                "DigitalPro Sistemas",
                "SmartBiz Soluções",
                "CloudTech Inovações"
            };
            return empresas[random.Next(empresas.Length)];
        }

        private string CreateCTeXml(int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<CTe xmlns=""http://www.portalfiscal.inf.br/cte"">
    <infCte>
        <ide>
            <cUF>35</cUF>
            <cCT>{numero:D8}</cCT>
            <CFOP>5353</CFOP>
            <natOp>Transporte</natOp>
            <mod>57</mod>
            <serie>1</serie>
            <nCT>{numero:D6}</nCT>
            <dhEmi>{DateTime.Now.AddDays(-numero):yyyy-MM-ddTHH:mm:ssK}</dhEmi>
        </ide>
        <emit>
            <CNPJ>14200166000187</CNPJ>
            <xNome>LOG CT-e Transportes Ltda</xNome>
        </emit>
        <vPrest>
            <vTPrest>{new Random().Next(1000, 5000)}.00</vTPrest>
        </vPrest>
    </infCte>
</CTe>";
        }

        private string CreateNFeXml(int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe>
        <ide>
            <cUF>35</cUF>
            <cNF>{numero:D8}</cNF>
            <natOp>Venda</natOp>
            <mod>55</mod>
            <serie>1</serie>
            <nNF>{numero:D6}</nNF>
            <dhEmi>{DateTime.Now.AddDays(-numero):yyyy-MM-ddTHH:mm:ssK}</dhEmi>
        </ide>
        <emit>
            <CNPJ>07850000000123</CNPJ>
            <xNome>TechSolutions Informática Ltda</xNome>
        </emit>
        <total>
            <ICMSTot>
                <vNF>{new Random().Next(2000, 15000)}.00</vNF>
            </ICMSTot>
        </total>
    </infNFe>
</NFe>";
        }

        private string CreateMDFeXml(int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<MDFe xmlns=""http://www.portalfiscal.inf.br/mdfe"">
    <infMDFe>
        <ide>
            <cUF>35</cUF>
            <tpAmb>2</tpAmb>
            <mod>58</mod>
            <serie>1</serie>
            <nMDF>{numero:D6}</nMDF>
            <dhEmi>{DateTime.Now.AddDays(-numero):yyyy-MM-ddTHH:mm:ssK}</dhEmi>
        </ide>
        <emit>
            <CNPJ>14200166000187</CNPJ>
            <xNome>LOG CT-e Transportes Ltda</xNome>
        </emit>
    </infMDFe>
</MDFe>";
        }

        private string CreateSimpleXml(string tipo, int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<documento>
    <tipo>{tipo}</tipo>
    <numero>{numero}</numero>
    <data>{DateTime.Now.AddDays(-numero):yyyy-MM-dd}</data>
</documento>";
        }

        private void LogSeedResults(List<DocumentoFiscal> documentos)
        {
            _logger.LogInformation("✅ Seed concluído! {Count} documentos adicionados", documentos.Count);
            _logger.LogInformation("📊 CT-e: {CTe}", documentos.Count(d => d.Tipo == TipoDocumento.CTe));
            _logger.LogInformation("📊 NF-e: {NFe}", documentos.Count(d => d.Tipo == TipoDocumento.NFe));
            _logger.LogInformation("📊 MDF-e: {MDFe}", documentos.Count(d => d.Tipo == TipoDocumento.MDFe));
            _logger.LogInformation("📊 Total de valor: {Valor:C}", documentos.Sum(d => d.ValorTotal));
        }
    }
}