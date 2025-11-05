using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class Transacao
    {
        public int IdTransacao { get; set; }
        public string DescricaoTransacao { get; set; }
        public string TipoTransacao { get; set; } // "Entrada" ou "Saída"
        public double ValorTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public string OrigemTransacao { get; set; }
        public int ParcelasTransacao { get; set; }
        public string FormaPagamento { get; set; }
        public string CondicaoPagamento { get; set; }

        // Métodos de persistência
        public async Task AdicionarTransacaoAsync()
        {
            var transacoes = await CarregarTransacoesAsync();
            this.IdTransacao = transacoes.Count > 0 ? transacoes.Max(t => t.IdTransacao) + 1 : 1;
            transacoes.Add(this);
            await SalvarTransacoesAsync(transacoes);
        }

        public async Task AlterarTransacaoAsync()
        {
            var transacoes = await CarregarTransacoesAsync();
            var transacaoExistente = transacoes.FirstOrDefault(t => t.IdTransacao == this.IdTransacao);
            if (transacaoExistente != null)
            {
                transacoes.Remove(transacaoExistente);
                transacoes.Add(this);
                await SalvarTransacoesAsync(transacoes);
            }
        }

        public async Task InativarTransacaoAsync()
        {
            // Implementação para marcar transação como inativa
            await AlterarTransacaoAsync();
        }

        // Métodos estáticos
        public static async Task<List<Transacao>> CarregarTransacoesAsync()
        {
            return await GerenciadorDados.CarregarAsync<List<Transacao>>("transacoes.json");
        }

        public static async Task SalvarTransacoesAsync(List<Transacao> transacoes)
        {
            await GerenciadorDados.SalvarAsync(transacoes, "transacoes.json");
        }

        public static async Task<List<Transacao>> CarregarTransacoesPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            var transacoes = await CarregarTransacoesAsync();
            return transacoes.Where(t => t.DataTransacao >= inicio && t.DataTransacao <= fim).ToList();
        }

        public void InserirParcelas()
        {
            // Implementação futura para transações parceladas
        }

        public void VisualizarTransacao()
        {
            // Implementação futura
        }
    }
}