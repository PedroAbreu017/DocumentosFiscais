namespace DocumentosFiscais.Data.Seed
{
    public static class XmlSampleHelper
    {
        public static string GetCTeXml(int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<CTe xmlns=""http://www.portalfiscal.inf.br/cte"">
    <infCte>
        <ide>
            <cUF>35</cUF>
            <cCT>{numero:D8}</cCT>
            <CFOP>5353</CFOP>
            <natOp>Transporte de Carga</natOp>
            <mod>57</mod>
            <serie>1</serie>
            <nCT>{numero:D6}</nCT>
            <dhEmi>{DateTime.Now.AddDays(-numero):yyyy-MM-ddTHH:mm:ssK}</dhEmi>
            <tpImp>1</tpImp>
            <tpEmis>1</tpEmis>
            <cDV>{numero % 10}</cDV>
            <tpAmb>2</tpAmb>
            <tpCTe>0</tpCTe>
            <procEmi>0</procEmi>
            <verProc>4.00</verProc>
        </ide>
        <emit>
            <CNPJ>14200166000187</CNPJ>
            <xNome>LOG CT-e Transportes Ltda</xNome>
            <xFant>LOG CT-e</xFant>
            <enderEmit>
                <xLgr>Rua das Transportadoras, 123</xLgr>
                <nro>123</nro>
                <xBairro>Centro</xBairro>
                <cMun>3550308</cMun>
                <xMun>São Paulo</xMun>
                <CEP>01234567</CEP>
                <UF>SP</UF>
            </enderEmit>
        </emit>
        <rem>
            <CNPJ>12345678000123</CNPJ>
            <xNome>Empresa Remetente Ltda</xNome>
        </rem>
        <dest>
            <CNPJ>98765432000198</CNPJ>
            <xNome>Empresa Destinatária SA</xNome>
        </dest>
        <vPrest>
            <vTPrest>{new Random().Next(1000, 5000)}.00</vTPrest>
            <vRec>{new Random().Next(1000, 5000)}.00</vRec>
        </vPrest>
        <imp>
            <ICMS>
                <ICMS00>
                    <CST>00</CST>
                    <vBC>1000.00</vBC>
                    <pICMS>12.00</pICMS>
                    <vICMS>120.00</vICMS>
                </ICMS00>
            </ICMS>
        </imp>
        <infCTeNorm>
            <infCarga>
                <vCarga>10000.00</vCarga>
                <proPred>Mercadoria Geral</proPred>
            </infCarga>
            <infDoc>
                <infNFe>
                    <chave>35220207850000000123550010000012341871234567</chave>
                </infNFe>
            </infDoc>
        </infCTeNorm>
    </infCte>
</CTe>";
        }

        public static string GetNFeXml(int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe>
        <ide>
            <cUF>35</cUF>
            <cNF>{numero:D8}</cNF>
            <natOp>Venda de Mercadoria</natOp>
            <mod>55</mod>
            <serie>1</serie>
            <nNF>{numero:D6}</nNF>
            <dhEmi>{DateTime.Now.AddDays(-numero):yyyy-MM-ddTHH:mm:ssK}</dhEmi>
            <tpNF>1</tpNF>
            <idDest>2</idDest>
            <cMunFG>3550308</cMunFG>
            <tpImp>1</tpImp>
            <tpEmis>1</tpEmis>
            <cDV>{numero % 10}</cDV>
            <tpAmb>2</tpAmb>
            <finNFe>1</finNFe>
            <indFinal>1</indFinal>
            <indPres>1</indPres>
            <procEmi>0</procEmi>
            <verProc>4.00</verProc>
        </ide>
        <emit>
            <CNPJ>07850000000123</CNPJ>
            <xNome>TechSolutions Informática Ltda</xNome>
            <xFant>TechSolutions</xFant>
            <enderEmit>
                <xLgr>Av. Tecnologia, 456</xLgr>
                <nro>456</nro>
                <xBairro>Tech Park</xBairro>
                <cMun>3550308</cMun>
                <xMun>São Paulo</xMun>
                <CEP>04567890</CEP>
                <UF>SP</UF>
            </enderEmit>
        </emit>
        <dest>
            <CNPJ>11222333000144</CNPJ>
            <xNome>Cliente Exemplo SA</xNome>
        </dest>
        <det nItem=""1"">
            <prod>
                <cProd>PROD{numero:D3}</cProd>
                <cEAN>7891234567890</cEAN>
                <xProd>Software de Gestão</xProd>
                <NCM>85234990</NCM>
                <CFOP>5102</CFOP>
                <uCom>UN</uCom>
                <qCom>1.0000</qCom>
                <vUnCom>{new Random().Next(5000, 20000)}.00</vUnCom>
                <vProd>{new Random().Next(5000, 20000)}.00</vProd>
                <cEANTrib>7891234567890</cEANTrib>
                <uTrib>UN</uTrib>
                <qTrib>1.0000</qTrib>
                <vUnTrib>{new Random().Next(5000, 20000)}.00</vUnTrib>
            </prod>
            <imposto>
                <ICMS>
                    <ICMS00>
                        <orig>0</orig>
                        <CST>00</CST>
                        <modBC>3</modBC>
                        <vBC>5000.00</vBC>
                        <pICMS>18.00</pICMS>
                        <vICMS>900.00</vICMS>
                    </ICMS00>
                </ICMS>
            </imposto>
        </det>
        <total>
            <ICMSTot>
                <vBC>5000.00</vBC>
                <vICMS>900.00</vICMS>
                <vICMSDeson>0.00</vICMSDeson>
                <vFCP>0.00</vFCP>
                <vBCST>0.00</vBCST>
                <vST>0.00</vST>
                <vFCPST>0.00</vFCPST>
                <vFCPSTRet>0.00</vFCPSTRet>
                <vProd>{new Random().Next(5000, 20000)}.00</vProd>
                <vFrete>0.00</vFrete>
                <vSeg>0.00</vSeg>
                <vDesc>0.00</vDesc>
                <vII>0.00</vII>
                <vIPI>0.00</vIPI>
                <vIPIDevol>0.00</vIPIDevol>
                <vPIS>0.00</vPIS>
                <vCOFINS>0.00</vCOFINS>
                <vOutro>0.00</vOutro>
                <vNF>{new Random().Next(5000, 20000)}.00</vNF>
            </ICMSTot>
        </total>
        <transp>
            <modFrete>0</modFrete>
        </transp>
        <pag>
            <detPag>
                <tPag>01</tPag>
                <vPag>{new Random().Next(5000, 20000)}.00</vPag>
            </detPag>
        </pag>
    </infNFe>
</NFe>";
        }

        public static string GetMDFeXml(int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<MDFe xmlns=""http://www.portalfiscal.inf.br/mdfe"">
    <infMDFe>
        <ide>
            <cUF>35</cUF>
            <tpAmb>2</tpAmb>
            <tpEmit>1</tpEmit>
            <tpTransp>1</tpTransp>
            <mod>58</mod>
            <serie>1</serie>
            <nMDF>{numero:D6}</nMDF>
            <cMDF>{numero:D8}</cMDF>
            <cDV>{numero % 10}</cDV>
            <modal>1</modal>
            <dhEmi>{DateTime.Now.AddDays(-numero):yyyy-MM-ddTHH:mm:ssK}</dhEmi>
            <tpEmis>1</tpEmis>
            <procEmi>0</procEmi>
            <verProc>4.00</verProc>
            <UFIni>SP</UFIni>
            <UFFim>RJ</UFFim>
        </ide>
        <emit>
            <CNPJ>14200166000187</CNPJ>
            <xNome>LOG CT-e Transportes Ltda</xNome>
            <xFant>LOG CT-e</xFant>
            <enderEmit>
                <xLgr>Rua das Transportadoras, 123</xLgr>
                <nro>123</nro>
                <xBairro>Centro</xBairro>
                <cMun>3550308</cMun>
                <xMun>São Paulo</xMun>
                <CEP>01234567</CEP>
                <UF>SP</UF>
            </enderEmit>
        </emit>
        <infModal versaoModal=""3.00"">
            <rodo>
                <infANTT>
                    <RNTRC>12345678</RNTRC>
                </infANTT>
                <veicTracao>
                    <cInt>001</cInt>
                    <placa>ABC1234</placa>
                    <RENAVAM>123456789</RENAVAM>
                    <tara>5000</tara>
                    <capKG>15000</capKG>
                    <capM3>30</capM3>
                    <prop>
                        <CPF>12345678901</CPF>
                        <xNome>João Motorista</xNome>
                    </prop>
                    <condutor>
                        <xNome>João Motorista</xNome>
                        <CPF>12345678901</CPF>
                    </condutor>
                    <tpRod>01</tpRod>
                    <tpCar>00</tpCar>
                    <UF>SP</UF>
                </veicTracao>
            </rodo>
        </infModal>
        <infDoc>
            <infMunDescarga>
                <cMunDescarga>3304557</cMunDescarga>
                <xMunDescarga>Rio de Janeiro</xMunDescarga>
                <infCTe>
                    <chCTe>35220114200166000187570010000{numero:D6}1871234{numero:D2}</chCTe>
                </infCTe>
            </infMunDescarga>
        </infDoc>
        <seg>
            <infResp>
                <respSeg>1</respSeg>
            </infResp>
        </seg>
        <prodPred>
            <tpCarga>01</tpCarga>
            <xProd>Mercadoria Geral</xProd>
            <cEAN>1234567890123</cEAN>
        </prodPred>
        <tot>
            <qCTe>1</qCTe>
            <qNFe>0</qNFe>
            <qMDFe>0</qMDFe>
            <vCarga>10000.00</vCarga>
            <cUnid>01</cUnid>
            <qCarga>1000.0000</qCarga>
        </tot>
    </infMDFe>
</MDFe>";
        }

        public static string GetSampleXml(string tipo, int numero)
        {
            return tipo switch
            {
                "CTe" => GetCTeXml(numero),
                "NFe" => GetNFeXml(numero),
                "MDFe" => GetMDFeXml(numero),
                _ => GetGenericXml(tipo, numero)
            };
        }

        private static string GetGenericXml(string tipo, int numero)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<documento>
    <tipo>{tipo}</tipo>
    <numero>{numero}</numero>
    <data>{DateTime.Now.AddDays(-numero):yyyy-MM-dd}</data>
    <versao>1.0</versao>
</documento>";
        }
    }
}