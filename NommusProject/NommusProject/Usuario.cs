using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public enum TipoUsuario
    {
        Basic = 1,
        Premium = 2,
        Adm = 3
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string telefone { get; set; }
        public string senha { get; set; }
        public TipoUsuario Tipo { get; set; }
        public bool idAdm { get; set; }
    
        public double saldoDisponivel { get; set; }


        public virtual void ExecutarAcao()
        {
            Console.WriteLine("Usuário padrão executando ação.");
        }
        public void calcularSaldo()
        {
           
        } 

    }
}