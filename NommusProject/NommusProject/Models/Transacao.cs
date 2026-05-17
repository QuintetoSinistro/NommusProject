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
        public string TipoTransacao { get; set; } // "Receita" ou "Despesa"
        public double ValorTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public string Local { get; set; } // Local onde ocorreu a transação
        public int ParcelasTransacao { get; set; } = 1;
        public string FormaPagamento { get; set; }
        public string CondicaoPagamento { get; set; } = "À vista";
        public string Observacao { get; set; } = string.Empty;

        // RELACIONAMENTOS PESSOAIS
        public int UsuarioId { get; set; }
        public string CategoriaId { get; set; }
        public int? CartaoId { get; set; }
        public bool TransacaoFixa { get; set; } = false; // Para despesas recorrentes

        // Métodos de persistência
        public async Task AdicionarTransacaoAsync()
        {
            var transacoes = await CarregarTransacoesAsync();
            this.IdTransacao = transacoes.Count > 0 ? transacoes.Max(t => t.IdTransacao) + 1 : 1;
            transacoes.Add(this);
            await SalvarTransacoesAsync(transacoes);

            // Atualiza saldo do usuário
            await AtualizarSaldoUsuarioAsync();
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

                // Recalcula saldo total
                await RecalcularSaldoUsuarioAsync(this.UsuarioId);
            }
        }

        public async Task ExcluirTransacaoAsync()
        {
            var transacoes = await CarregarTransacoesAsync();
            var transacaoExistente = transacoes.FirstOrDefault(t => t.IdTransacao == this.IdTransacao);
            if (transacaoExistente != null)
            {
                transacoes.Remove(transacaoExistente);
                await SalvarTransacoesAsync(transacoes);
                await RecalcularSaldoUsuarioAsync(this.UsuarioId);
            }
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

        // Métodos com filtros pessoais
        public static async Task<List<Transacao>> CarregarTransacoesPorUsuarioAsync(int usuarioId)
        {
            var transacoes = await CarregarTransacoesAsync();
            return transacoes.Where(t => t.UsuarioId == usuarioId)
                           .OrderByDescending(t => t.DataTransacao)
                           .ToList();
        }

        public static async Task<List<Transacao>> CarregarTransacoesPorPeriodoAsync(int usuarioId, DateTime inicio, DateTime fim)
        {
            var transacoes = await CarregarTransacoesPorUsuarioAsync(usuarioId);
            return transacoes.Where(t => t.DataTransacao >= inicio && t.DataTransacao <= fim).ToList();
        }

        public static async Task<List<Transacao>> CarregarTransacoesRecentesAsync(int usuarioId, int quantidade = 10)
        {
            var transacoes = await CarregarTransacoesPorUsuarioAsync(usuarioId);
            return transacoes.Take(quantidade).ToList();
        }

        public static async Task<List<Transacao>> CarregarTransacoesFixasAsync(int usuarioId)
        {
            var transacoes = await CarregarTransacoesPorUsuarioAsync(usuarioId);
            return transacoes.Where(t => t.TransacaoFixa).ToList();
        }

        // Métodos auxiliares para saldo pessoal
        private async Task AtualizarSaldoUsuarioAsync()
        {
            var usuario = await Usuario.BuscarUsuarioPorIdAsync(this.UsuarioId);
            if (usuario != null)
            {
                if (this.TipoTransacao == "Receita")
                {
                    usuario.saldoDisponivel += this.ValorTransacao;
                }
                else
                {
                    usuario.saldoDisponivel -= this.ValorTransacao;
                }
                await usuario.SalvarUsuarioAsync();
            }
        }

        private async Task RecalcularSaldoUsuarioAsync(int usuarioId)
        {
            var usuario = await Usuario.BuscarUsuarioPorIdAsync(usuarioId);
            if (usuario != null)
            {
                var transacoes = await CarregarTransacoesPorUsuarioAsync(usuarioId);
                double saldo = 0;

                foreach (var transacao in transacoes)
                {
                    if (transacao.TipoTransacao == "Receita")
                    {
                        saldo += transacao.ValorTransacao;
                    }
                    else
                    {
                        saldo -= transacao.ValorTransacao;
                    }
                }

                usuario.saldoDisponivel = saldo;
                await usuario.SalvarUsuarioAsync();
            }
        }

        // Método para criar transações parceladas (compras no cartão)
        public async Task<List<Transacao>> CriarTransacoesParceladasAsync()
        {
            var transacoesParceladas = new List<Transacao>();

            for (int i = 0; i < this.ParcelasTransacao; i++)
            {
                var parcela = new Transacao
                {
                    DescricaoTransacao = $"{this.DescricaoTransacao} ({i + 1}/{this.ParcelasTransacao})",
                    TipoTransacao = this.TipoTransacao,
                    ValorTransacao = this.ValorTransacao / this.ParcelasTransacao,
                    DataTransacao = this.DataTransacao.AddMonths(i),
                    Local = this.Local,
                    FormaPagamento = "Cartão de Crédito",
                    CondicaoPagamento = "Parcelado",
                    UsuarioId = this.UsuarioId,
                    CategoriaId = this.CategoriaId,
                    CartaoId = this.CartaoId,
                    Observacao = this.Observacao
                };

                transacoesParceladas.Add(parcela);
            }

            return transacoesParceladas;
        }
    }
}