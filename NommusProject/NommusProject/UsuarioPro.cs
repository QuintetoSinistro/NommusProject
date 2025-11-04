using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
     public class UsuarioPro: Usuario
    {
        public int relatóriAnual { get; set; }

        public string alertasAtivados { get; set; }

        public UsuarioPro()
        {
            this.Tipo = TipoUsuario.Premium;
        }
        public void AcessoPro()
        {
            Console.WriteLine("Acesso Pro: Funcionalidades avançadas.");
        }
        public override void ExecutarAcao()
        {
            Console.WriteLine("Usuário Pro executando ação com funcionalidades avançadas.");
        }
        public void realizarCadastro()
        {

        }

        public void gerarDashBoarts()
        {

        }
    }
}
