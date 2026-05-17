using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace NommusProject.Data
{
    // Repositório responsável pelas operações de banco de dados relacionadas às metas de economia do usuário.
    public class MetasRepository
    {
        // String de conexão com o banco SQLite (o arquivo .db fica em %APPDATA%\NommusApp\nommus.db)
        private readonly string _connectionString;

        public MetasRepository()
        {
            _connectionString = $"Data Source={DatabaseInitializer.DbPath}";
        }

        // ============================================================
        // CONEXÃO COM O BANCO (ativa chaves estrangeiras)
        // ============================================================

        // Cria e abre uma conexão, ativando a verificação de chaves estrangeiras (IdUsuario deve existir em Usuarios).
        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        // ============================================================
        // INSERÇÃO DE META
        // ============================================================

        /// <summary>
        /// Adiciona uma nova meta de economia para o usuário.
        /// A propriedade IdMeta será preenchida com o valor gerado pelo autoincremento.
        /// </summary>
        public void Add(Metas meta)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO Metas (NomeMeta, ValorMeta, DataInicial, DataFinal, StatusMeta, IdUsuario, ValorAtual)
                VALUES (@Nome, @Valor, @DataIni, @DataFim, @Status, @IdUsuario, @ValorAtual);
                SELECT last_insert_rowid();";  // Retorna o ID da nova linha

            using var cmd = new SqliteCommand(sql, connection);

            // Parâmetros da query
            cmd.Parameters.AddWithValue("@Nome", meta.NomeMeta);
            cmd.Parameters.AddWithValue("@Valor", meta.ValorMeta);
            // Formata a data no padrão ISO (yyyy-MM-dd) para compatibilidade com SQLite
            cmd.Parameters.AddWithValue("@DataIni", meta.DataInicial.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@DataFim", meta.DataFinal.ToString("yyyy-MM-dd"));
            // Converte bool para 1 (true) ou 0 (false)
            cmd.Parameters.AddWithValue("@Status", meta.StatusMeta ? 1 : 0);
            cmd.Parameters.AddWithValue("@IdUsuario", meta.IdUsuario);
            cmd.Parameters.AddWithValue("@ValorAtual", meta.ValorAtual);

            // Executa e atribui o ID gerado (last_insert_rowid) à propriedade IdMeta do objeto
            meta.IdMeta = (int)(long)cmd.ExecuteScalar();
        }

        // ============================================================
        // ATUALIZAÇÃO DE META
        // ============================================================

        /// <summary>
        /// Atualiza uma meta existente (ex: para modificar o valor atual economizado ou o status).
        /// </summary>
        public void Update(Metas meta)
        {
            using var connection = GetConnection();
            var sql = @"UPDATE Metas SET NomeMeta=@Nome, ValorMeta=@Valor, DataInicial=@DataIni,
                DataFinal=@DataFim, StatusMeta=@Status, IdUsuario=@IdUsuario, ValorAtual=@ValorAtual
                WHERE IdMeta=@Id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nome", meta.NomeMeta);
            cmd.Parameters.AddWithValue("@Valor", meta.ValorMeta);
            cmd.Parameters.AddWithValue("@DataIni", meta.DataInicial.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@DataFim", meta.DataFinal.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Status", meta.StatusMeta ? 1 : 0);
            cmd.Parameters.AddWithValue("@IdUsuario", meta.IdUsuario);
            cmd.Parameters.AddWithValue("@ValorAtual", meta.ValorAtual);
            cmd.Parameters.AddWithValue("@Id", meta.IdMeta);

            cmd.ExecuteNonQuery();
        }

        // ============================================================
        // EXCLUSÃO DE META
        // ============================================================

        /// <summary>
        /// Remove uma meta pelo seu Id. Usado quando o usuário deseja apagar uma meta.
        /// </summary>
        public void Delete(int id)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("DELETE FROM Metas WHERE IdMeta=@Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <summary>
        /// Busca uma meta específica pelo seu Id.
        /// Retorna null se não existir.
        /// </summary>
        public Metas GetById(int id)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Metas WHERE IdMeta=@Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return Map(reader);
            return null;
        }

        /// <summary>
        /// Retorna todas as metas pertencentes a um determinado usuário.
        /// Usado na tela de Metas para exibir a lista do usuário logado.
        /// </summary>
        public List<Metas> GetByUsuario(int usuarioId)
        {
            var list = new List<Metas>();
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Metas WHERE IdUsuario=@IdUsuario", connection);
            cmd.Parameters.AddWithValue("@IdUsuario", usuarioId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        // ============================================================
        // MAPEAMENTO DE DADOS (SqliteDataReader -> Objeto Metas)
        // ============================================================

        // Converte a linha atual do leitor em um objeto Metas.
        // As posições das colunas são baseadas na ordem do SELECT *.
        private Metas Map(SqliteDataReader reader)
        {
            return new Metas
            {
                IdMeta = reader.GetInt32(0),          // Coluna 0: IdMeta
                NomeMeta = reader.GetString(1),       // Coluna 1: NomeMeta
                ValorMeta = reader.GetDouble(2),      // Coluna 2: ValorMeta
                DataInicial = DateTime.Parse(reader.GetString(3)),  // Coluna 3: DataInicial (yyyy-MM-dd)
                DataFinal = DateTime.Parse(reader.GetString(4)),    // Coluna 4: DataFinal
                StatusMeta = reader.GetInt32(5) == 1, // Coluna 5: StatusMeta (0 = false, 1 = true)
                IdUsuario = reader.GetInt32(6),       // Coluna 6: IdUsuario (chave estrangeira)
                ValorAtual = reader.IsDBNull(7) ? 0 : reader.GetDouble(7)  // Coluna 7: ValorAtual (pode ser NULL)
            };
        }
    }
}