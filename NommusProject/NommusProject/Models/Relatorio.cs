using System.Collections.Generic;

namespace NommusProject
{
    public static class RelatorioManager
    {
        public class ResumoMensal
        {
            public double TotalReceitas { get; set; }
            public double TotalDespesas { get; set; }
            public double SaldoMensal { get; set; }
            public double EconomiaMensal { get; set; }
            public double PercentualEconomia { get; set; }
            public List<CategoriaResumo> DespesasPorCategoria { get; set; } = new List<CategoriaResumo>();
            public List<Transacao> UltimasTransacoes { get; set; } = new List<Transacao>();
        }

        public class CategoriaResumo
        {
            public string CategoriaNome { get; set; }
            public string CategoriaCor { get; set; }
            public double Total { get; set; }
            public double Percentual { get; set; }
            public string Tipo { get; set; }
        }

        public class EvolucaoPatrimonial
        {
            public string Mes { get; set; }
            public double Receitas { get; set; }
            public double Despesas { get; set; }
            public double Saldo { get; set; }
            public double Patrimonio { get; set; }
        }

        public class SugestaoEconomia
        {
            public string Categoria { get; set; }
            public double ValorMensal { get; set; }
            public string Sugestao { get; set; }
            public double PotencialEconomia { get; set; }
        }

        public class ProjecaoEconomia
        {
            public double EconomiaMediaMensal { get; set; }
            public double MetaSugerida { get; set; }
            public int MesesParaMeta { get; set; }
            public bool PossivelAlcancarMeta { get; set; }
        }
    }
}