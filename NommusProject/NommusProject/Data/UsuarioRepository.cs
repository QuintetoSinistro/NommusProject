using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NommusProject;

namespace NommusProject.Data
{
    internal class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository()
        {
            _connectionString = $"Data Source={DatabaseInitializer.DbPath}";
        }

        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            // Habilita suporte a chaves estrangeiras (importante para outras tabelas)
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        public void Add(Usuarios usuario)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO Usuarios (Nome, Email, Telefone, Senha, Tipo, IdAdm, SaldoDisponivel)
                        VALUES (@Nome, @Email, @Telefone, @Senha, @Tipo, @IdAdm, @Saldo);
                        SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Telefone", usuario.telefone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Senha", usuario.senha);
            cmd.Parameters.AddWithValue("@Tipo", (int)usuario.Tipo);
            cmd.Parameters.AddWithValue("@IdAdm", usuario.idAdm ? 1 : 0);
            cmd.Parameters.AddWithValue("@Saldo", usuario.saldoDisponivel);

            // Obtém o ID gerado e atribui ao objeto
            usuario.Id = (int)(long)cmd.ExecuteScalar();
        }

        public void Update(Usuarios usuario)
        {
            using var connection = GetConnection();
            var sql = @"UPDATE Usuarios 
                        SET Nome = @Nome, Email = @Email, Telefone = @Telefone, 
                            Senha = @Senha, Tipo = @Tipo, IdAdm = @IdAdm, 
                            SaldoDisponivel = @Saldo
                        WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Telefone", usuario.telefone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Senha", usuario.senha);
            cmd.Parameters.AddWithValue("@Tipo", (int)usuario.Tipo);
            cmd.Parameters.AddWithValue("@IdAdm", usuario.idAdm ? 1 : 0);
            cmd.Parameters.AddWithValue("@Saldo", usuario.saldoDisponivel);
            cmd.Parameters.AddWithValue("@Id", usuario.Id);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = GetConnection();
            var sql = "DELETE FROM Usuarios WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public void AtualizarSaldo(int usuarioId, double valor, bool adicionar)
        {
            var usuario = GetById(usuarioId);
            if (usuario == null) return;

            if (adicionar)
                usuario.saldoDisponivel += valor;
            else
                usuario.saldoDisponivel -= valor;

            Update(usuario);
            // Atualiza o objeto em SessaoUsuario se necessário
            if (SessaoUsuario.UsuarioLogado?.Id == usuarioId)
                SessaoUsuario.UsuarioLogado.saldoDisponivel = usuario.saldoDisponivel;
        }

        public Usuarios GetById(int id)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Usuarios WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToUsuario(reader);
            return null;
        }

        public Usuarios GetByEmail(string email)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Usuarios WHERE Email = @Email;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToUsuario(reader);
            return null;
        }

        public List<Usuarios> GetAll()
        {
            var lista = new List<Usuarios>();
            using var connection = GetConnection();
            var sql = "SELECT * FROM Usuarios;";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(MapToUsuario(reader));
            return lista;
        }

        private Usuarios MapToUsuario(SqliteDataReader reader)
        {
            return new Usuarios
            {
                Id = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Email = reader.GetString(2),
                telefone = reader.IsDBNull(3) ? null : reader.GetString(3),
                senha = reader.GetString(4),
                Tipo = (TipoUsuario)reader.GetInt32(5),
                idAdm = reader.GetInt32(6) == 1,
                saldoDisponivel = reader.GetDouble(7)
            };
        }

        internal Usuarios BuscarPorEmailESenha(string email, string senha)
        {
            throw new NotImplementedException();
        }
    }
}