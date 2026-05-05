using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace NommusProject.Data
{
    public class TransacaoRepository
    {
        private readonly string _connectionString;

        public TransacaoRepository()
        {
            _connectionString = $"Data Source={DatabaseInitializer.DbPath}";
        }

        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        public int Add(Transacao transacao)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO Transacoes 
                (DescricaoTransacao, TipoTransacao, ValorTransacao, DataTransacao, 
                 ParcelasTransacao, FormaPagamento, CondicaoPagamento, 
                 IdUsuario, IdCategoria, IdCartao)
                VALUES (@Descricao, @Tipo, @Valor, @Data, @Parcelas, @FormaPgto, @CondPgto,
                        @IdUsuario, @IdCategoria, @IdCartao);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Descricao", transacao.DescricaoTransacao ?? "");
            cmd.Parameters.AddWithValue("@Tipo", transacao.TipoTransacao);
            cmd.Parameters.AddWithValue("@Valor", transacao.ValorTransacao);
            cmd.Parameters.AddWithValue("@Data", transacao.DataTransacao.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Parcelas", transacao.ParcelasTransacao);
            cmd.Parameters.AddWithValue("@FormaPgto", transacao.FormaPagamento ?? "");
            cmd.Parameters.AddWithValue("@CondPgto", transacao.CondicaoPagamento ?? "");
            cmd.Parameters.AddWithValue("@IdUsuario", transacao.UsuarioId);
            cmd.Parameters.AddWithValue("@IdCategoria", transacao.CategoriaId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IdCartao", transacao.CartaoId ?? (object)DBNull.Value);

            return (int)(long)cmd.ExecuteScalar();
        }

        public void Update(Transacao transacao)
        {
            using var connection = GetConnection();
            var sql = @"UPDATE Transacoes SET 
                DescricaoTransacao=@Descricao, TipoTransacao=@Tipo, ValorTransacao=@Valor, 
                DataTransacao=@Data, ParcelasTransacao=@Parcelas, FormaPagamento=@FormaPgto, 
                CondicaoPagamento=@CondPgto, IdUsuario=@IdUsuario, IdCategoria=@IdCategoria, 
                IdCartao=@IdCartao
                WHERE IdTransacao=@Id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Descricao", transacao.DescricaoTransacao ?? "");
            cmd.Parameters.AddWithValue("@Tipo", transacao.TipoTransacao);
            cmd.Parameters.AddWithValue("@Valor", transacao.ValorTransacao);
            cmd.Parameters.AddWithValue("@Data", transacao.DataTransacao.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Parcelas", transacao.ParcelasTransacao);
            cmd.Parameters.AddWithValue("@FormaPgto", transacao.FormaPagamento ?? "");
            cmd.Parameters.AddWithValue("@CondPgto", transacao.CondicaoPagamento ?? "");
            cmd.Parameters.AddWithValue("@IdUsuario", transacao.UsuarioId);
            cmd.Parameters.AddWithValue("@IdCategoria", transacao.CategoriaId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IdCartao", transacao.CartaoId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", transacao.IdTransacao);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int idTransacao)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("DELETE FROM Transacoes WHERE IdTransacao=@Id", connection);
            cmd.Parameters.AddWithValue("@Id", idTransacao);
            cmd.ExecuteNonQuery();
        }

        public Transacao GetById(int id)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Transacoes WHERE IdTransacao=@Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return Map(reader);
            return null;
        }

        public List<Transacao> GetByUsuario(int usuarioId)
        {
            var list = new List<Transacao>();
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Transacoes WHERE IdUsuario=@IdUsuario ORDER BY DataTransacao DESC", connection);
            cmd.Parameters.AddWithValue("@IdUsuario", usuarioId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        public List<Transacao> GetByUsuarioAndTipo(int usuarioId, string tipo)
        {
            var list = new List<Transacao>();
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Transacoes WHERE IdUsuario=@IdUsuario AND TipoTransacao=@Tipo ORDER BY DataTransacao DESC", connection);
            cmd.Parameters.AddWithValue("@IdUsuario", usuarioId);
            cmd.Parameters.AddWithValue("@Tipo", tipo);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        private Transacao Map(SqliteDataReader reader)
        {
            return new Transacao
            {
                IdTransacao = reader.GetInt32(0),
                DescricaoTransacao = reader.GetString(1),
                TipoTransacao = reader.GetString(2),
                ValorTransacao = reader.GetDouble(3),
                DataTransacao = DateTime.Parse(reader.GetString(4)),
                ParcelasTransacao = reader.GetInt32(5),
                FormaPagamento = reader.GetString(6),
                CondicaoPagamento = reader.GetString(7),
                UsuarioId = reader.GetInt32(8),
                CategoriaId = reader.IsDBNull(9) ? null : reader.GetString(9),
                CartaoId = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10)
            };
        }
    }
}