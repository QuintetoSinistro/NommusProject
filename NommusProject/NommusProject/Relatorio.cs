using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public static class RelatorioManager
    {
        public class ResumoMensal
        {
            public double TotalReceitas { get; set; }
            public double TotalDespesas { get; set; }
            public double SaldoMensal { get; set; }
            public double EconomiaMensal { get; set; } // Quanto sobrou
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
            public double Patrimonio { get; set; } // Saldo acumulado
        }

        // Relatório mensal pessoal
        public static async Task<ResumoMensal> GerarResumoMensalAsync(int usuarioId, int mes = 0, int ano = 0)
        {
            if (mes == 0) mes = DateTime.Now.Month;
            if (ano == 0) ano = DateTime.Now.Year;

            var inicioMes = new DateTime(ano, mes, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var transacoes = await Transacao.CarregarTransacoesPorPeriodoAsync(usuarioId, inicioMes, fimMes);

            var receitas = transacoes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao);
            var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);
            var saldo = receitas - despesas;
            var percentualEconomia = receitas > 0 ? (saldo / receitas) * 100 : 0;

            var resumo = new ResumoMensal
            {
                TotalReceitas = receitas,
                TotalDespesas = despesas,
                SaldoMensal = saldo,
                EconomiaMensal = saldo,
                PercentualEconomia = percentualEconomia,
                UltimasTransacoes = transacoes.OrderByDescending(t => t.DataTransacao).Take(10).ToList()
            };

            // Calcular despesas por categoria
            resumo.DespesasPorCategoria = await CalcularDespesasPorCategoriaAsync(usuarioId, inicioMes, fimMes);

            return resumo;
        }

        // Análise de despesas por categoria
        private static async Task<List<CategoriaResumo>> CalcularDespesasPorCategoriaAsync(int usuarioId, DateTime inicio, DateTime fim)
        {
            var transacoes = await Transacao.CarregarTransacoesPorPeriodoAsync(usuarioId, inicio, fim);
            var categorias = await Categoria.CarregarCategoriasAsync();
            var totalDespesas = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);

            var resultado = transacoes
                .Where(t => t.TipoTransacao == "Despesa")
                .GroupBy(t => t.CategoriaId)
                .Select(g => new CategoriaResumo
                {
                    CategoriaNome = categorias.FirstOrDefault(c => c.IdCategoria == g.Key)?.NomeCategoria ?? "Outros",
                    CategoriaCor = categorias.FirstOrDefault(c => c.IdCategoria == g.Key)?.CorCategoria ?? "#95a5a6",
                    Total = g.Sum(t => t.ValorTransacao),
                    Tipo = "Despesa"
                })
                .Where(tc => tc.Total > 0)
                .OrderByDescending(tc => tc.Total)
                .ToList();

            // Calcular percentuais
            foreach (var item in resultado)
            {
                item.Percentual = totalDespesas > 0 ? (item.Total / totalDespesas) * 100 : 0;
            }

            return resultado;
        }

        // Evolução do patrimônio (saldo acumulado)
        public static async Task<List<EvolucaoPatrimonial>> GerarEvolucaoPatrimonialAsync(int usuarioId, int meses = 12)
        {
            var evolucao = new List<EvolucaoPatrimonial>();
            var dataFim = DateTime.Now;
            var dataInicio = dataFim.AddMonths(-meses);
            double patrimonioAcumulado = 0;

            for (DateTime data = dataInicio; data <= dataFim; data = data.AddMonths(1))
            {
                var inicioMes = new DateTime(data.Year, data.Month, 1);
                var fimMes = inicioMes.AddMonths(1).AddDays(-1);

                var transacoes = await Transacao.CarregarTransacoesPorPeriodoAsync(usuarioId, inicioMes, fimMes);
                var receitas = transacoes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao);
                var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);
                var saldoMes = receitas - despesas;

                patrimonioAcumulado += saldoMes;

                evolucao.Add(new EvolucaoPatrimonial
                {
                    Mes = inicioMes.ToString("MMM/yyyy"),
                    Receitas = receitas,
                    Despesas = despesas,
                    Saldo = saldoMes,
                    Patrimonio = patrimonioAcumulado
                });
            }

            return evolucao;
        }

        // Método para sugerir economias
        public static async Task<List<SugestaoEconomia>> GerarSugestoesEconomiaAsync(int usuarioId)
        {
            var sugestoes = new List<SugestaoEconomia>();
            var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(usuarioId);
            var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa").ToList();

            // Analisa despesas não essenciais
            var despesasNaoEssenciais = despesas.Where(d =>
                d is Despesa despesa && !despesa.DespesaEssencial)
                .OrderByDescending(d => d.ValorTransacao)
                .Take(3);

            foreach (var despesa in despesasNaoEssenciais)
            {
                sugestoes.Add(new SugestaoEconomia
                {
                    Categoria = despesa.DescricaoTransacao,
                    ValorMensal = despesa.ValorTransacao,
                    Sugestao = $"Reduzir gastos com {despesa.DescricaoTransacao}",
                    PotencialEconomia = despesa.ValorTransacao * 0.3 // Sugere reduzir 30%
                });
            }

            return sugestoes;
        }

        public class SugestaoEconomia
        {
            public string Categoria { get; set; }
            public double ValorMensal { get; set; }
            public string Sugestao { get; set; }
            public double PotencialEconomia { get; set; }
        }

        // Método para calcular projeção de economia
        public static async Task<ProjecaoEconomia> CalcularProjecaoEconomiaAsync(int usuarioId, double metaMensal)
        {
            var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(usuarioId);
            var ultimosMeses = transacoes.Where(t => t.DataTransacao >= DateTime.Now.AddMonths(-3));

            var mediaReceitas = ultimosMeses.Where(t => t.TipoTransacao == "Receita")
                                          .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                                          .Average(g => g.Sum(t => t.ValorTransacao));

            var mediaDespesas = ultimosMeses.Where(t => t.TipoTransacao == "Despesa")
                                          .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                                          .Average(g => g.Sum(t => t.ValorTransacao));

            var economiaMedia = mediaReceitas - mediaDespesas;
            var mesesParaMeta = metaMensal > 0 ? metaMensal / economiaMedia : 0;

            return new ProjecaoEconomia
            {
                EconomiaMediaMensal = economiaMedia,
                MetaSugerida = economiaMedia * 0.8, // 80% da economia média
                MesesParaMeta = (int)Math.Ceiling(mesesParaMeta),
                PossivelAlcancarMeta = economiaMedia > 0
            };
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