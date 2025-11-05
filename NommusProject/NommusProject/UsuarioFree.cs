using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class UsuarioFree : Usuario
    {
        public int relatórioMensais { get; set; }

        public UsuarioFree()
        {
            this.Tipo = TipoUsuario.Basic;
        }

        public void AcessoFree()
        {
            Console.WriteLine("Acesso Free: Funcionalidades limitadas.");
        }

        public override void ExecutarAcao()
        {
            Console.WriteLine("Usuário Free executando ação com limitações.");
        }

        // Método específico para cadastro Free
        public async Task<bool> RealizarCadastroFreeAsync(string nome, string email, string senha, string telefone)
        {
            this.Nome = nome;
            this.Email = email;
            this.senha = senha;
            this.telefone = telefone;
            this.Tipo = TipoUsuario.Basic;
            this.saldoDisponivel = 0;
            this.relatórioMensais = 1; // Relatório mensal básico

            return await this.SalvarUsuarioAsync();
        }

        public void GerarRelatorioMensal()
        {
            Console.WriteLine($"Gerando relatório mensal para usuário Free: {this.Nome}");
            // Implementação específica do relatório Free
        }
    }
}
