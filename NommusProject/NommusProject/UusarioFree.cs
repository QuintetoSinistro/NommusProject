using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    class UusarioFree: Usuario
    {
        public int relatórioMensais { get; set; }

        public UusarioFree()
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

        public void realizarCadastro()
        {

        }
    }
}
