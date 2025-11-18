using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class Receita : Transacao
    {
        public string FonteReceita { get; set; } = string.Empty; // Salário, Freelance, etc
        public bool ReceitaRecorrente { get; set; } = false;

        public Receita()
        {
            this.TipoTransacao = "Receita";
            this.FormaPagamento = "Depósito";
        }

        public async Task AdicionarReceitaAsync(int usuarioId, string categoriaId = "receita-salario")
        {
            this.UsuarioId = usuarioId;
            this.CategoriaId = categoriaId;
            this.DataTransacao = DateTime.Now;
            await this.AdicionarTransacaoAsync();
        }

        // Métodos específicos para receitas
        public static async Task<double> CalcularTotalReceitasAsync(int usuarioId, DateTime? inicio = null, DateTime? fim = null)
        {
            var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(usuarioId);
            var receitas = transacoes.Where(t => t.TipoTransacao == "Receita");

            if (inicio.HasValue && fim.HasValue)
            {
                receitas = receitas.Where(t => t.DataTransacao >= inicio.Value && t.DataTransacao <= fim.Value);
            }

            return receitas.Sum(t => t.ValorTransacao);
        }

        public static async Task<List<Receita>> CarregarReceitasRecorrentesAsync(int usuarioId)
        {
            var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(usuarioId);
            return transacoes.Where(t => t is Receita receita && receita.ReceitaRecorrente)
                           .Cast<Receita>()
                           .ToList();
        }
    }
}