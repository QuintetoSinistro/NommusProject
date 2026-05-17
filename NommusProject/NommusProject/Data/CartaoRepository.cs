using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace NommusProject.Data
{
    public class CartaoRepository
    {
        private readonly string _connectionString;

        public CartaoRepository() : this($"Data Source={DatabaseInitializer.DbPath}") { }

        public CartaoRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        private async Task<SqliteConnection> GetConnectionAsync()
        {
            var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            await pragmaCmd.ExecuteNonQueryAsync();
            return conn;
        }

        public void Add(Cartao cartao)
        {
            if (cartao == null) throw new ArgumentNullException(nameof(cartao));

            using var connection = GetConnection();
            var sql = @"INSERT INTO Cartoes (NomeCartao, LimiteCartao, DataVencimento, BandeiraCartao, IdUsuario, NumeroCartao)
                        VALUES (@Nome, @Limite, @Vencimento, @Bandeira, @IdUsuario, @Numero);
                        SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.Add(new SqliteParameter("@Nome", SqliteType.Text) { Value = cartao.NomeCartao ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqliteParameter("@Limite", SqliteType.Real) { Value = cartao.LimiteCartao });
            cmd.Parameters.Add(new SqliteParameter("@Vencimento", SqliteType.Text) { Value = cartao.DataVencimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
            cmd.Parameters.Add(new SqliteParameter("@Bandeira", SqliteType.Text) { Value = cartao.BandeiraCartao ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqliteParameter("@IdUsuario", SqliteType.Integer) { Value = cartao.IdUsuario });
            cmd.Parameters.Add(new SqliteParameter("@Numero", SqliteType.Text) { Value = cartao.NumeroCartao ?? (object)DBNull.Value });

            var result = cmd.ExecuteScalar();
            if (result is long lastId)
                cartao.IdCartao = (int)lastId;
            else
                throw new InvalidOperationException("Não foi possível obter o Id gerado pelo banco.");
        }

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
                                BandeiraCartao = @Bandeira,
                                NumeroCartao = @Numero
                            WHERE IdCartao = @Id";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.Add(new SqliteParameter("@Nome", SqliteType.Text) { Value = cartao.NomeCartao ?? (object)DBNull.Value });
                cmd.Parameters.Add(new SqliteParameter("@Limite", SqliteType.Real) { Value = cartao.LimiteCartao });
                cmd.Parameters.Add(new SqliteParameter("@Vencimento", SqliteType.Text) { Value = cartao.DataVencimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
                cmd.Parameters.Add(new SqliteParameter("@Bandeira", SqliteType.Text) { Value = cartao.BandeiraCartao ?? (object)DBNull.Value });
                cmd.Parameters.Add(new SqliteParameter("@Numero", SqliteType.Text) { Value = cartao.NumeroCartao ?? (object)DBNull.Value });
                cmd.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) { Value = cartao.IdCartao });

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

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

        public Cartao GetById(int id)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT IdCartao, NomeCartao, LimiteCartao, DataVencimento, BandeiraCartao, NumeroCartao, IdUsuario FROM Cartoes WHERE IdCartao = @Id", connection);
            cmd.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) { Value = id });

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return Map(reader);
            return null;
        }

        public List<Cartao> GetAll()
        {
            var list = new List<Cartao>();
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT IdCartao, NomeCartao, LimiteCartao, DataVencimento, BandeiraCartao, NumeroCartao, IdUsuario FROM Cartoes", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

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

        private Cartao Map(SqliteDataReader reader)
        {
            int idxId = reader.GetOrdinal("IdCartao");
            int idxNome = reader.GetOrdinal("NomeCartao");
            int idxLimite = reader.GetOrdinal("LimiteCartao");
            int idxVenc = reader.GetOrdinal("DataVencimento");
            int idxBandeira = reader.GetOrdinal("BandeiraCartao");
            int idxUsuario = reader.GetOrdinal("IdUsuario");
            int idxNumero = reader.GetOrdinal("NumeroCartao");

            var cartao = new Cartao
            {
                IdCartao = !reader.IsDBNull(idxId) ? reader.GetInt32(idxId) : 0,
                NomeCartao = !reader.IsDBNull(idxNome) ? reader.GetString(idxNome) : null,
                LimiteCartao = !reader.IsDBNull(idxLimite) ? reader.GetDouble(idxLimite) : 0.0,
                BandeiraCartao = !reader.IsDBNull(idxBandeira) ? reader.GetString(idxBandeira) : null,
                IdUsuario = reader.GetInt32(idxUsuario),
                NumeroCartao = reader.IsDBNull(idxNumero) ? null : reader.GetString(idxNumero)
            };

            if (!reader.IsDBNull(idxVenc))
            {
                var vencStr = reader.GetString(idxVenc);
                if (DateTime.TryParseExact(vencStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    cartao.DataVencimento = dt;
                else if (DateTime.TryParse(vencStr, out dt))
                    cartao.DataVencimento = dt;
                else
                    cartao.DataVencimento = DateTime.MinValue;
            }
            else
                cartao.DataVencimento = DateTime.MinValue;

            return cartao;
        }
    }
}