using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class UsuarioPro : Usuario
    {
        public int relatóriAnual { get; set; }

        public string alertasAtivados { get; set; }

        public UsuarioPro()
        {
            this.Tipo = TipoUsuario.Premium;
            this.alertasAtivados = "Sim";
        }

        public void AcessoPro()
        {
            Console.WriteLine("Acesso Pro: Funcionalidades avançadas.");
        }

        public override void ExecutarAcao()
        {
            Console.WriteLine("Usuário Pro executando ação com funcionalidades avançadas.");
        }

        // Método específico para cadastro Pro
        public async Task<bool> RealizarCadastroProAsync(string nome, string email, string senha, string telefone)
        {
            this.Nome = nome;
            this.Email = email;
            this.senha = senha;
            this.telefone = telefone;
            this.Tipo = TipoUsuario.Premium;
            this.saldoDisponivel = 0;
            this.relatóriAnual = 1; // Relatório anual
            this.alertasAtivados = "Sim";

            return await this.SalvarUsuarioAsync();
        }

        public void GerarDashboards()
        {
            Console.WriteLine($"Gerando dashboard avançado para usuário Pro: {this.Nome}");
            // Implementação específica dos dashboards Pro
        }

        public void AtivarAlertas(string tipoAlerta)
        {
            this.alertasAtivados = tipoAlerta;
            Console.WriteLine($"Alertas ativados: {tipoAlerta}");
        }
    }
}
