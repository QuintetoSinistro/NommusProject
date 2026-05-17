using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using BCrypt.Net;

namespace NommusProject
{
    // Enum que define os tipos de usuário do sistema
    public enum TipoUsuario
    {
        Basic = 1,    // Usuário comum (funcionalidades básicas)
        Premium = 2,  // Usuário com funcionalidades avançadas (futuro)
        Adm = 3       // Administrador (acesso total)
    }

    // Modelo (entidade) que representa um usuário no sistema
    public class Usuarios
    {
        // ============================================================
        // PROPRIEDADES (mapeiam as colunas da tabela Usuarios no banco)
        // ============================================================

        public int Id { get; set; }                  // Chave primária, auto incremento
        public string Nome { get; set; }             // Nome completo do usuário
        public string Email { get; set; }            // Email (único, usado para login)
        public string telefone { get; set; }         // Telefone para contato
        public string senha { get; set; }            // Senha (ARMAZENADA EM TEXTO PLANO – NÃO SEGURO)
        public TipoUsuario Tipo { get; set; }        // Tipo do usuário (Basic/Premium/Adm)
        public bool idAdm { get; set; }              // Flag redundante (poderia ser removida, pois Tipo já define)
        public double saldoDisponivel { get; set; }  // Saldo atual do usuário (calculado ou armazenado)
        public string FotoPerfil { get; set; } // caminho da imagem ou base64

        // Repositório estático compartilhado por todas as instâncias (acesso ao banco de dados)
        private static readonly UsuarioRepository _repository = new UsuarioRepository();

        // ============================================================
        // MÉTODOS DE INSTÂNCIA (OPERAM NO OBJETO ATUAL)
        // ============================================================

        /// <summary>
        /// Salva o usuário atual no banco de dados (inserção se Id = 0, atualização caso contrário).
        /// </summary>
        /// <returns>True se a operação foi bem-sucedida, False em caso de erro.</returns>
        public bool Salvar()
        {
            try
            {
                if (this.Id == 0)
                    _repository.Add(this);      // Novo registro: o repositório atribui o Id gerado
                else
                    _repository.Update(this);   // Atualiza registro existente
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Exclui o usuário atual do banco de dados.
        /// </summary>
        /// <returns>True se a exclusão foi bem-sucedida, False em caso de erro.</returns>
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

        // ============================================================
        // MÉTODOS ESTÁTICOS (OPERAM NO REPOSITÓRIO)
        // ============================================================

        /// <summary>
        /// Retorna uma lista com todos os usuários cadastrados.
        /// </summary>
        public static List<Usuarios> CarregarTodos()
        {
            return _repository.GetAll();
        }

        /// <summary>
        /// Busca um usuário pelo email (utilizado no login e na verificação de duplicidade).
        /// </summary>
        public static Usuarios BuscarPorEmail(string email)
        {
            return _repository.GetByEmail(email);
        }

        /// <summary>
        /// Busca um usuário pelo seu ID.
        /// </summary>
        public static Usuarios BuscarPorId(int id)
        {
            return _repository.GetById(id);
        }

        // ============================================================
        // WRAPPERS ASSÍNCRONOS (COMPATIBILIDADE COM MÉTODOS ASSÍNCRONOS NAS TELAS)
        // ============================================================

        /// <summary>
        /// Versão assíncrona do método Salvar().
        /// Executa em background usando Task.Run.
        /// </summary>
        public async Task<bool> SalvarAsync() => await Task.Run(() => Salvar());

        /// <summary>
        /// Versão assíncrona de CarregarTodos().
        /// </summary>
        public static async Task<List<Usuarios>> CarregarTodosAsync()
            => await Task.Run(() => CarregarTodos());

        /// <summary>
        /// Versão assíncrona de BuscarPorEmail() – utilizada nas telas de login e cadastro.
        /// </summary>
        public static async Task<Usuarios> BuscarUsuarioPorEmailAsync(string email)
            => await Task.Run(() => BuscarPorEmail(email));

        /// <summary>
        /// Versão assíncrona de BuscarPorId().
        /// </summary>
        public static async Task<Usuarios> BuscarUsuarioPorIdAsync(int id)
            => await Task.Run(() => BuscarPorId(id));

        public void DefinirSenha(string senhaPlain)
        {
            senha = BCrypt.Net.BCrypt.HashPassword(senhaPlain);
        }

        public bool VerificarSenha(string senhaPlain)
        {
            return BCrypt.Net.BCrypt.Verify(senhaPlain, senha);
        }
    }
}