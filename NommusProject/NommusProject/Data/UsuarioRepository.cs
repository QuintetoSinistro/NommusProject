using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject.Data
{
    public class UsuarioRepository
    {
        // String de conexão com o banco de dados SQLite.
        // O caminho do arquivo .db é obtido de DatabaseInitializer.DbPath (normalmente em %APPDATA%\NommusApp\nommus.db)
        private readonly string _connectionString;

        // Construtor: define a string de conexão usando o caminho padrão do banco de dados.
        public UsuarioRepository()
        {
            _connectionString = $"Data Source={DatabaseInitializer.DbPath}";
        }

        // ============================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================

        // Cria e abre uma conexão com o banco de dados, ativando a verificação de chaves estrangeiras.
        // Importante para garantir integridade referencial em operações que envolvem outras tabelas (Transacoes, Cartoes, Metas).
        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        // ============================================================
        // OPERAÇÕES CRUD BÁSICAS
        // ============================================================

        /// <summary>
        /// Insere um novo usuário no banco de dados.
        /// O parâmetro usuario terá sua propriedade Id preenchida com o valor gerado pelo autoincremento.
        /// </summary>
        public void Add(Usuarios usuario)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO Usuarios (Nome, Email, Telefone, Senha, Tipo, IdAdm, SaldoDisponivel)
                        VALUES (@Nome, @Email, @Telefone, @Senha, @Tipo, @IdAdm, @Saldo);
                        SELECT last_insert_rowid();";  // Retorna o ID gerado

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            // Se telefone for nulo, envia DBNull.Value para o banco (NULL)
            cmd.Parameters.AddWithValue("@Telefone", usuario.telefone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Senha", usuario.senha);               // ATENÇÃO: senha em texto puro!
            cmd.Parameters.AddWithValue("@Tipo", (int)usuario.Tipo);            // Converte enum para inteiro
            cmd.Parameters.AddWithValue("@IdAdm", usuario.idAdm ? 1 : 0);       // Booleano vira 0 ou 1
            cmd.Parameters.AddWithValue("@Saldo", usuario.saldoDisponivel);

            // Executa e obtém o ID gerado (last_insert_rowid) como long, depois converte para int.
            usuario.Id = (int)(long)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Atualiza os dados de um usuário existente (identificado pelo Id).
        /// </summary>
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

        /// <summary>
        /// Remove um usuário do banco de dados pelo seu Id.
        /// Cuidado: as chaves estrangeiras (ON DELETE RESTRICT/NO ACTION) podem impedir a exclusão se houver registros relacionados.
        /// </summary>
        public void Delete(int id)
        {
            using var connection = GetConnection();
            var sql = "DELETE FROM Usuarios WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================================================
        // OPERAÇÕES DE CONSULTA E RECUPERAÇÃO
        // ============================================================

        /// <summary>
        /// Busca um usuário pelo seu Id.
        /// Retorna null se não existir.
        /// </summary>
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

        /// <summary>
        /// Busca um usuário pelo email (deve ser único).
        /// Usado no login e na verificação de cadastro.
        /// </summary>
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

        /// <summary>
        /// Retorna todos os usuários cadastrados no sistema (útil para administradores).
        /// </summary>
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

        // ============================================================
        // OPERAÇÕES ESPECÍFICAS
        // ============================================================

        /// <summary>
        /// Atualiza o saldo disponível de um usuário (incrementa ou decrementa).
        /// Também atualiza o objeto na sessão global (SessaoUsuario) se for o usuário logado.
        /// </summary>
        /// <param name="usuarioId">Id do usuário</param>
        /// <param name="valor">Valor a ser adicionado ou subtraído</param>
        /// <param name="adicionar">True = adiciona, False = subtrai</param>
        public void AtualizarSaldo(int usuarioId, double valor, bool adicionar)
        {
            var usuario = GetById(usuarioId);
            if (usuario == null) return;

            if (adicionar)
                usuario.saldoDisponivel += valor;
            else
                usuario.saldoDisponivel -= valor;

            Update(usuario);   // Persiste a alteração no banco

            // Se o usuário sendo atualizado é o mesmo que está logado, atualiza a sessão também
            if (SessaoUsuario.UsuarioLogado?.Id == usuarioId)
                SessaoUsuario.UsuarioLogado.saldoDisponivel = usuario.saldoDisponivel;
        }

        /// <summary>
        /// Busca um usuário por email e senha (utilizado para login – menos eficiente que GetByEmail + verificação separada,
        /// mas útil para alguns cenários). **SENHA EM TEXTO PURO – NÃO SEGURO.**
        /// </summary>
        public Usuarios BuscarPorEmailESenha(string email, string senha)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Usuarios WHERE Email = @Email AND Senha = @Senha;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Senha", senha);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToUsuario(reader);
            return null;
        }

        // ============================================================
        // Mapeamento de dados (SqliteDataReader -> Objeto Usuarios)
        // ============================================================

        // Converte uma linha do resultado da consulta SQL em um objeto Usuarios.
        // As posições das colunas são baseadas na ordem do SELECT *.
        private Usuarios MapToUsuario(SqliteDataReader reader)
        {
            return new Usuarios
            {
                Id = reader.GetInt32(0),                // Coluna 0: Id
                Nome = reader.GetString(1),              // Coluna 1: Nome
                Email = reader.GetString(2),             // Coluna 2: Email
                telefone = reader.IsDBNull(3) ? null : reader.GetString(3), // Coluna 3: Telefone (pode ser NULL)
                senha = reader.GetString(4),             // Coluna 4: Senha (texto puro)
                Tipo = (TipoUsuario)reader.GetInt32(5),  // Coluna 5: Tipo (inteiro convertido para enum)
                idAdm = reader.GetInt32(6) == 1,         // Coluna 6: IdAdm (0 = false, 1 = true)
                saldoDisponivel = reader.GetDouble(7)    // Coluna 7: SaldoDisponivel
            };
        }
    }
}