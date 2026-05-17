using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace NommusProject.Data
{
    // Repositório responsável pelas operações de banco de dados relacionadas a cartões de crédito/débito do usuário.
    public class CartaoRepository
    {
        // String de conexão com o banco SQLite (o arquivo .db fica em %APPDATA%\NommusApp\nommus.db)
        private readonly string _connectionString;

        // Construtor padrão: usa o caminho definido em DatabaseInitializer.DbPath
        public CartaoRepository()
            : this($"Data Source={DatabaseInitializer.DbPath}")
        {
        }

        // Construtor que permite injeção de string de conexão (útil para testes unitários)
        public CartaoRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // ============================================================
        // MÉTODOS DE CONEXÃO (síncronos e assíncronos)
        // ============================================================

        // Cria e abre uma conexão síncrona, ativando as chaves estrangeiras.
        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        // Versão assíncrona de GetConnection (para operações que precisam de escalabilidade, embora não seja essencial para um app desktop)
        private async Task<SqliteConnection> GetConnectionAsync()
        {
            var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            await pragmaCmd.ExecuteNonQueryAsync();
            return conn;
        }

        // ============================================================
        // OPERAÇÕES CRUD (Create, Read, Update, Delete)
        // ============================================================

        /// <summary>
        /// Insere um novo cartão no banco de dados.
        /// O parâmetro cartao terá sua propriedade IdCartao preenchida com o valor gerado pelo autoincremento.
        /// </summary>
        /// <param name="cartao">Objeto Cartao com dados a serem inseridos.</param>
        /// <exception cref="ArgumentNullException">Lançada se cartao for nulo.</exception>
        /// <exception cref="InvalidOperationException">Lançada se o ID gerado não for obtido corretamente.</exception>
        public void Add(Cartao cartao)
        {
            if (cartao == null) throw new ArgumentNullException(nameof(cartao));

            using var connection = GetConnection();
            var sql = @"INSERT INTO Cartoes (NomeCartao, LimiteCartao, DataVencimento, BandeiraCartao, IdUsuario)
                        VALUES (@Nome, @Limite, @Vencimento, @Bandeira, @IdUsuario);
                        SELECT last_insert_rowid();"; // Retorna o ID da nova linha

            using var cmd = new SqliteCommand(sql, connection);

            // Adiciona os parâmetros com os tipos corretos
            cmd.Parameters.Add(new SqliteParameter("@Nome", SqliteType.Text) { Value = cartao.NomeCartao ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqliteParameter("@Limite", SqliteType.Real) { Value = cartao.LimiteCartao });
            // Formata a data de vencimento no padrão ISO (yyyy-MM-dd) com cultura invariante
            cmd.Parameters.Add(new SqliteParameter("@Vencimento", SqliteType.Text)
            {
                Value = cartao.DataVencimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
            cmd.Parameters.Add(new SqliteParameter("@Bandeira", SqliteType.Text) { Value = cartao.BandeiraCartao ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqliteParameter("@IdUsuario", SqliteType.Integer) { Value = cartao.IdUsuario });

            var result = cmd.ExecuteScalar();
            if (result is long lastId)
            {
                cartao.IdCartao = (int)lastId;
            }
            else
            {
                throw new InvalidOperationException("Não foi possível obter o Id gerado pelo banco.");
            }
        }

        /// <summary>
        /// Atualiza um cartão existente (nome, limite, vencimento, bandeira).
        /// </summary>
        /// <param name="cartao">Objeto com os novos dados (deve conter IdCartao válido).</param>
        /// <returns>True se a atualização foi bem-sucedida (afetou pelo menos uma linha), False caso contrário.</returns>
        public async Task<bool> Update(Cartao cartao)
        {
            if (cartao == null) throw new ArgumentNullException(nameof(cartao));

            var connection = await GetConnectionAsync();
            using (connection)
            {
                var sql = @"UPDATE Cartoes
                            SET NomeCartao = @Nome,
                                LimiteCartao = @Limite,
                                DataVencimento = @Vencimento,
                                BandeiraCartao = @Bandeira
                            WHERE IdCartao = @Id";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.Add(new SqliteParameter("@Nome", SqliteType.Text) { Value = cartao.NomeCartao ?? (object)DBNull.Value });
                cmd.Parameters.Add(new SqliteParameter("@Limite", SqliteType.Real) { Value = cartao.LimiteCartao });
                cmd.Parameters.Add(new SqliteParameter("@Vencimento", SqliteType.Text)
                {
                    Value = cartao.DataVencimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                });
                cmd.Parameters.Add(new SqliteParameter("@Bandeira", SqliteType.Text) { Value = cartao.BandeiraCartao ?? (object)DBNull.Value });
                cmd.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) { Value = cartao.IdCartao });

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0; // Indica sucesso se alguma linha foi modificada
            }
        }

        /// <summary>
        /// Remove um cartão do banco de dados pelo seu Id.
        /// </summary>
        /// <param name="id">Id do cartão a ser removido.</param>
        /// <returns>True se a exclusão foi bem-sucedida (afetou uma linha), False caso contrário.</returns>
        public async Task<bool> Delete(int id)
        {
            var connection = await GetConnectionAsync();
            using (connection)
            {
                using var cmd = new SqliteCommand("DELETE FROM Cartoes WHERE IdCartao = @Id", connection);
                cmd.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) { Value = id });

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <summary>
        /// Busca um cartão específico pelo seu Id.
        /// Retorna null se não existir.
        /// </summary>
        public Cartao GetById(int id)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand(
                "SELECT IdCartao, NomeCartao, LimiteCartao, DataVencimento, BandeiraCartao FROM Cartoes WHERE IdCartao = @Id",
                connection);
            cmd.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) { Value = id });

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return Map(reader);
            return null;
        }

        /// <summary>
        /// Retorna todos os cartões cadastrados no sistema (independentemente do usuário).
        /// Útil apenas para administradores ou para testes.
        /// </summary>
        public List<Cartao> GetAll()
        {
            var list = new List<Cartao>();
            using var connection = GetConnection();
            using var cmd = new SqliteCommand(
                "SELECT IdCartao, NomeCartao, LimiteCartao, DataVencimento, BandeiraCartao FROM Cartoes",
                connection);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        /// <summary>
        /// Retorna apenas os cartões pertencentes a um determinado usuário.
        /// Usado na tela de cartões e no combo de seleção de cartão nas despesas.
        /// </summary>
        public List<Cartao> GetByUsuario(int usuarioId)
        {
            var list = new List<Cartao>();
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Cartoes WHERE IdUsuario = @IdUsuario", connection);
            cmd.Parameters.AddWithValue("@IdUsuario", usuarioId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        // ============================================================
        // MAPEAMENTO DE DADOS (SqliteDataReader -> Objeto Cartao)
        // ============================================================

        // Converte a linha atual do leitor em um objeto Cartao.
        // Utiliza os nomes das colunas (mais robusto que ordinais) e trata valores nulos.
        private Cartao Map(SqliteDataReader reader)
        {
            // Obtém os índices das colunas por nome (permite mudanças na ordem do SELECT)
            int idxId = reader.GetOrdinal("IdCartao");
            int idxNome = reader.GetOrdinal("NomeCartao");
            int idxLimite = reader.GetOrdinal("LimiteCartao");
            int idxVenc = reader.GetOrdinal("DataVencimento");
            int idxBandeira = reader.GetOrdinal("BandeiraCartao");

            var cartao = new Cartao
            {
                IdCartao = !reader.IsDBNull(idxId) ? reader.GetInt32(idxId) : 0,
                NomeCartao = !reader.IsDBNull(idxNome) ? reader.GetString(idxNome) : null,
                LimiteCartao = !reader.IsDBNull(idxLimite) ? reader.GetDouble(idxLimite) : 0.0,
                BandeiraCartao = !reader.IsDBNull(idxBandeira) ? reader.GetString(idxBandeira) : null
            };

            // Tratamento especial para a data de vencimento (pode ser nula)
            if (!reader.IsDBNull(idxVenc))
            {
                var vencStr = reader.GetString(idxVenc);
                // Tenta parse exato no formato ISO (yyyy-MM-dd)
                if (DateTime.TryParseExact(vencStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    cartao.DataVencimento = dt;
                }
                // Fallback: parse genérico (caso venha em outro formato)
                else if (DateTime.TryParse(vencStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    cartao.DataVencimento = dt;
                }
                else
                {
                    cartao.DataVencimento = DateTime.MinValue;
                }
            }
            else
            {
                cartao.DataVencimento = DateTime.MinValue;
            }

            return cartao;
        }
    }
}