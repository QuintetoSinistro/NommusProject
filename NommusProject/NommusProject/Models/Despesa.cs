using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class Despesa : Transacao
    {
        public bool DespesaEssencial { get; set; } = true; // Se é essencial ou supérflua
        public bool DespesaRecorrente { get; set; } = false; // Para despesas fixas mensais

        public Despesa()
        {
            this.TipoTransacao = "Despesa";
            this.FormaPagamento = "Dinheiro";
        }

        public async Task AdicionarDespesaAsync(int usuarioId, string categoriaId = "despesa-alimentacao")
        {
            this.UsuarioId = usuarioId;
            this.CategoriaId = categoriaId;
            await this.AdicionarTransacaoAsync();
        }

        // Métodos específicos para despesas
        public static async Task<double> CalcularTotalDespesasAsync(int usuarioId, DateTime? inicio = null, DateTime? fim = null)
        {
            var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(usuarioId);
            var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa");

            if (inicio.HasValue && fim.HasValue)
            {
                despesas = despesas.Where(t => t.DataTransacao >= inicio.Value && t.DataTransacao <= fim.Value);
            }

            return despesas.Sum(t => t.ValorTransacao);
        }

        public static async Task<List<Despesa>> CarregarDespesasFixasAsync(int usuarioId)
        {
            var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(usuarioId);
            return transacoes.Where(t => t is Despesa despesa && despesa.DespesaRecorrente)
                           .Cast<Despesa>()
                           .ToList();
        }

        // Método para verificar se a despesa está dentro do orçamento
        public async Task<bool> EstaDentroDoOrcamentoAsync(int usuarioId, double orcamentoMensal)
        {
            var despesasMes = await CalcularTotalDespesasAsync(usuarioId,
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                DateTime.Now);

            return (despesasMes + this.ValorTransacao) <= orcamentoMensal;
        }
    }
}