using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NommusProject.Data;

namespace NommusProject
{
    public enum TipoUsuario
    {
        Basic = 1,
        Premium = 2,
        Adm = 3
    }

    public class Usuarios
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string telefone { get; set; }
        public string senha { get; set; }
        public TipoUsuario Tipo { get; set; }
        public bool idAdm { get; set; }
        public double saldoDisponivel { get; set; }

        private static readonly UsuarioRepository _repository = new UsuarioRepository();

        // --- Métodos de instância ---
        public bool Salvar()
        {
            try
            {
                if (this.Id == 0)
                    _repository.Add(this);      // atribui Id automaticamente
                else
                    _repository.Update(this);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Excluir()
        {
            try
            {
                _repository.Delete(this.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // --- Métodos estáticos ---
        public static List<Usuarios> CarregarTodos()
        {
            return _repository.GetAll();
        }

        public static Usuarios BuscarPorEmail(string email)
        {
            return _repository.GetByEmail(email);
        }

        public static Usuarios BuscarPorId(int id)
        {
            return _repository.GetById(id);
        }

        // --- Wrappers assíncronos (opcionais, para compatibilidade) ---
        public async Task<bool> SalvarAsync() => await Task.Run(() => Salvar());
        public static async Task<List<Usuarios>> CarregarTodosAsync()
            => await Task.Run(() => CarregarTodos());
        public static async Task<Usuarios> BuscarUsuarioPorEmailAsync(string email)
            => await Task.Run(() => BuscarPorEmail(email));
        public static async Task<Usuarios> BuscarUsuarioPorIdAsync(int id)
            => await Task.Run(() => BuscarPorId(id));
    }
}