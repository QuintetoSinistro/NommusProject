using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class Cartao
    {
        public int IdCartao { get; set; }
        public string NomeCartao { get; set; }
        public double LimiteCartao { get; set; }
        public DateTime DataVencimento { get; set; }
        public string BandeiraCartao { get; set; }

        // Métodos de persistência
        public async Task AdicionarCartaoAsync()
        {
            var cartoes = await CarregarCartoesAsync();
            this.IdCartao = cartoes.Count > 0 ? cartoes.Max(c => c.IdCartao) + 1 : 1;
            cartoes.Add(this);
            await SalvarCartoesAsync(cartoes);
        }

        public async Task AlterarCartaoAsync()
        {
            var cartoes = await CarregarCartoesAsync();
            var cartaoExistente = cartoes.FirstOrDefault(c => c.IdCartao == this.IdCartao);
            if (cartaoExistente != null)
            {
                cartoes.Remove(cartaoExistente);
                cartoes.Add(this);
                await SalvarCartoesAsync(cartoes);
            }
        }

        public async Task ExcluirCartaoAsync()
        {
            var cartoes = await CarregarCartoesAsync();
            var cartaoExistente = cartoes.FirstOrDefault(c => c.IdCartao == this.IdCartao);
            if (cartaoExistente != null)
            {
                cartoes.Remove(cartaoExistente);
                await SalvarCartoesAsync(cartoes);
            }
        }

        // Métodos estáticos para gerenciar a lista
        public static async Task<List<Cartao>> CarregarCartoesAsync()
        {
            return await GerenciadorDados.CarregarAsync<List<Cartao>>("cartoes.json");
        }
        public static async Task SalvarCartoesAsync(List<Cartao> cartoes)
        {
            await GerenciadorDados.SalvarAsync(cartoes, "cartoes.json");
        }
    }
}