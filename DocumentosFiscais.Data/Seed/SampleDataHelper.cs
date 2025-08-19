namespace DocumentosFiscais.Data.Seed
{
    public static class SampleDataHelper
    {
        public static string GetRandomTransportadora(Random random)
        {
            var transportadoras = new[]
            {
                "LOG CT-e Transportes Ltda",
                "TransLog Soluções Logísticas",
                "RodoLog Transportes SA",
                "ViaLog Logística Integrada",
                "CargoLog Transportes",
                "FastLog Expressos",
                "EcoLog Sustentável",
                "MegaLog Transportes",
                "PrimeLog Logística",
                "UltraLog Express"
            };
            return transportadoras[random.Next(transportadoras.Length)];
        }

        public static string GetRandomEmpresa(Random random)
        {
            var empresas = new[]
            {
                "TechSolutions Informática Ltda",
                "InnovaCorp Tecnologia SA",
                "DigitalPro Sistemas",
                "SmartBiz Soluções",
                "CloudTech Inovações",
                "DataCorp Analytics",
                "SoftwarePlus Desenvolvimento",
                "NextGen Tecnologia",
                "FutureTech Soluções",
                "ProCode Sistemas"
            };
            return empresas[random.Next(empresas.Length)];
        }

        public static string GenerateRandomCnpj(Random random)
        {
            var digits = new int[14];
            
            // Gerar os primeiros 12 dígitos
            for (int i = 0; i < 12; i++)
            {
                digits[i] = random.Next(0, 10);
            }
            
            // Calcular primeiro dígito verificador
            digits[12] = CalculateCnpjDigit(digits, new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
            
            // Calcular segundo dígito verificador
            digits[13] = CalculateCnpjDigit(digits, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
            
            return $"{digits[0]}{digits[1]}.{digits[2]}{digits[3]}{digits[4]}.{digits[5]}{digits[6]}{digits[7]}/{digits[8]}{digits[9]}{digits[10]}{digits[11]}-{digits[12]}{digits[13]}";
        }

        private static int CalculateCnpjDigit(int[] digits, int[] weights)
        {
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += digits[i] * weights[i];
            }
            
            int remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }

        public static string GenerateRandomHash()
        {
            return Guid.NewGuid().ToString("N")[..32];
        }

        public static DateTime GetRandomDateInRange(DateTime start, DateTime end, Random random)
        {
            var range = end - start;
            var randomSpan = new TimeSpan((long)(random.NextDouble() * range.Ticks));
            return start + randomSpan;
        }

        public static string GenerateDocumentNumber(int sequence, string prefix = "")
        {
            return $"{prefix}{sequence:D8}";
        }

        public static long GenerateFileSize(Random random, int minKb = 30, int maxKb = 500)
        {
            return random.Next(minKb * 1024, maxKb * 1024);
        }
    }
}