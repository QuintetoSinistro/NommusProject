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
            // Implementação futura
        }

        // Métodos de persistência
        public async Task<bool> SalvarUsuarioAsync()
        {
            try
            {
                var usuarios = await CarregarTodosUsuariosAsync();

                if (this.Id == 0)
                {
                    this.Id = usuarios.Count > 0 ? usuarios.Max(u => u.Id) + 1 : 1;
                    usuarios.Add(this);
                }
                else
                {
                    var usuarioExistente = usuarios.FirstOrDefault(u => u.Id == this.Id);
                    if (usuarioExistente != null)
                    {
                        usuarios.Remove(usuarioExistente);
                    }
                    usuarios.Add(this);
                }

                await SalvarTodosUsuariosAsync(usuarios);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExcluirUsuarioAsync()
        {
            try
            {
                var usuarios = await CarregarTodosUsuariosAsync();
                var usuarioExistente = usuarios.FirstOrDefault(u => u.Id == this.Id);
                if (usuarioExistente != null)
                {
                    usuarios.Remove(usuarioExistente);
                    await SalvarTodosUsuariosAsync(usuarios);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Métodos estáticos para gerenciar todos os usuários
        public static async Task<List<Usuario>> CarregarTodosUsuariosAsync()
        {
            return await GerenciadorDados.CarregarAsync<List<Usuario>>("usuarios.json");
        }

        public static async Task SalvarTodosUsuariosAsync(List<Usuario> usuarios)
        {
            await GerenciadorDados.SalvarAsync(usuarios, "usuarios.json");
        }

        public static async Task<Usuario> BuscarUsuarioPorEmailAsync(string email)
        {
            var usuarios = await CarregarTodosUsuariosAsync();
            return usuarios.FirstOrDefault(u => u.Email == email);
        }

        public static async Task<Usuario> BuscarUsuarioPorIdAsync(int id)
        {
            var usuarios = await CarregarTodosUsuariosAsync();
            return usuarios.FirstOrDefault(u => u.Id == id);
        }

        // Métodos específicos por tipo
        public static async Task<List<UsuarioFree>> CarregarUsuariosFreeAsync()
        {
            var usuarios = await CarregarTodosUsuariosAsync();
            return usuarios.OfType<UsuarioFree>().ToList();
        }

        public static async Task<List<UsuarioPro>> CarregarUsuariosProAsync()
        {
            var usuarios = await CarregarTodosUsuariosAsync();
            return usuarios.OfType<UsuarioPro>().ToList();
        }
    }
}