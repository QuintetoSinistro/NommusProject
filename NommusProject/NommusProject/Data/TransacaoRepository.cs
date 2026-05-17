using Microsoft.Data.Sqlite;

namespace NommusProject.Data
{
    // Repositório responsável pelas operações de banco de dados relacionadas às transações (receitas/despesas).
    public class TransacaoRepository
    {
        // String de conexão com o banco SQLite (o arquivo .db fica em %APPDATA%\NommusApp\nommus.db)
        private readonly string _connectionString;

        public TransacaoRepository()
        {
            _connectionString = $"Data Source={DatabaseInitializer.DbPath}";
        }

        // ============================================================
        // CONEXÃO COM O BANCO (ativa chaves estrangeiras)
        // ============================================================

        // Cria e abre uma conexão, além de ativar a verificação de chaves estrangeiras (importante para integridade referencial).
        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        // ============================================================
        // INSERÇÃO DE TRANSAÇÃO (Receita ou Despesa)
        // ============================================================

        /// <summary>
        /// Adiciona uma nova transação (receita ou despesa) ao banco de dados.
        /// Retorna o IdTransacao gerado pelo autoincremento.
        /// </summary>
        public int Add(Transacao transacao)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO Transacoes 
                (DescricaoTransacao, TipoTransacao, ValorTransacao, DataTransacao, OrigemTransacao,
                 ParcelasTransacao, FormaPagamento, CondicaoPagamento, 
                 IdUsuario, IdCategoria, IdCartao)
                VALUES (@Descricao, @Tipo, @Valor, @Data, @Origem, @Parcelas, @FormaPgto, @CondPgto,
                        @IdUsuario, @IdCategoria, @IdCartao);
                SELECT last_insert_rowid();"; // Retorna o ID da linha inserida

            using var cmd = new SqliteCommand(sql, connection);

            // Parâmetros obrigatórios (nunca nulos)
            cmd.Parameters.AddWithValue("@Descricao", transacao.DescricaoTransacao ?? "");
            cmd.Parameters.AddWithValue("@Tipo", transacao.TipoTransacao ?? "");
            cmd.Parameters.AddWithValue("@Valor", transacao.ValorTransacao);
            cmd.Parameters.AddWithValue("@Data", transacao.DataTransacao.ToString("yyyy-MM-dd")); // Formato ISO
            cmd.Parameters.AddWithValue("@Origem", transacao.Local ?? (object)DBNull.Value);      // Se nulo, vira NULL
            cmd.Parameters.AddWithValue("@Parcelas", transacao.ParcelasTransacao);
            cmd.Parameters.AddWithValue("@FormaPgto", transacao.FormaPagamento ?? "");
            cmd.Parameters.AddWithValue("@CondPgto", transacao.CondicaoPagamento ?? "");
            cmd.Parameters.AddWithValue("@IdUsuario", transacao.UsuarioId);

            // Tratamento especial para CategoriaId (pode ser null ou 0)
            if (transacao.CategoriaId.HasValue && transacao.CategoriaId.Value > 0)
                cmd.Parameters.AddWithValue("@IdCategoria", transacao.CategoriaId.Value);
            else
                cmd.Parameters.AddWithValue("@IdCategoria", DBNull.Value); // NULL no banco

            // Tratamento especial para CartaoId (pode ser null ou 0)
            if (transacao.CartaoId.HasValue && transacao.CartaoId.Value > 0)
                cmd.Parameters.AddWithValue("@IdCartao", transacao.CartaoId.Value);
            else
                cmd.Parameters.AddWithValue("@IdCartao", DBNull.Value);



            // Executa a inserção e retorna o ID gerado (converte long para int)
            int id = (int)(long)cmd.ExecuteScalar();
            AtualizarSaldoUsuario(transacao.UsuarioId);
            return id;
        }

        // ============================================================
        // ATUALIZAÇÃO DE TRANSAÇÃO (raro, mas mantido para completude)
        // ============================================================

        /// <summary>
        /// Atualiza uma transação existente. Normalmente não é usado, pois transações não são editadas.
        /// </summary>
        public void Update(Transacao transacao)
        {
            using var connection = GetConnection();
            var sql = @"UPDATE Transacoes SET 
                DescricaoTransacao=@Descricao, TipoTransacao=@Tipo, ValorTransacao=@Valor, 
                DataTransacao=@Data, OrigemTransacao=@Origem, ParcelasTransacao=@Parcelas, FormaPagamento=@FormaPgto, 
                CondicaoPagamento=@CondPgto, IdUsuario=@IdUsuario, IdCategoria=@IdCategoria, 
                IdCartao=@IdCartao
                WHERE IdTransacao=@Id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Descricao", transacao.DescricaoTransacao ?? "");
            cmd.Parameters.AddWithValue("@Tipo", transacao.TipoTransacao ?? "");
            cmd.Parameters.AddWithValue("@Valor", transacao.ValorTransacao);
            cmd.Parameters.AddWithValue("@Data", transacao.DataTransacao.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Origem", transacao.Local ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Parcelas", transacao.ParcelasTransacao);
            cmd.Parameters.AddWithValue("@FormaPgto", transacao.FormaPagamento ?? "");
            cmd.Parameters.AddWithValue("@CondPgto", transacao.CondicaoPagamento ?? "");
            cmd.Parameters.AddWithValue("@IdUsuario", transacao.UsuarioId);
            // Nota: aqui não foi feita a validação >0, mas poderia ser igual ao Add
            cmd.Parameters.AddWithValue("@IdCategoria", transacao.CategoriaId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IdCartao", transacao.CartaoId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", transacao.IdTransacao);

            cmd.ExecuteNonQuery();
        }

        private void AtualizarSaldoUsuario(int usuarioId)
        {
            var transacoes = GetByUsuario(usuarioId);
            double saldo = transacoes.Sum(t => t.TipoTransacao == "Receita" ? t.ValorTransacao : -t.ValorTransacao);

            var usuarioRepo = new UsuarioRepository();
            var usuario = usuarioRepo.GetById(usuarioId);
            if (usuario != null)
            {
                usuario.saldoDisponivel = saldo;
                usuarioRepo.Update(usuario);

                // Atualiza a sessão se for o usuário logado
                if (SessaoUsuario.UsuarioLogado?.Id == usuarioId)
                    SessaoUsuario.UsuarioLogado.saldoDisponivel = saldo;
            }
        }

        // ============================================================
        // EXCLUSÃO DE TRANSAÇÃO
        // ============================================================

        /// <summary>
        /// Remove uma transação pelo seu Id (usado nos botões "✕" da lista).
        /// </summary>
        public void Delete(int idTransacao)
        {
            var transacao = GetById(idTransacao);
            if (transacao == null) return;

            using var connection = GetConnection();
            using var cmd = new SqliteCommand("DELETE FROM Transacoes WHERE IdTransacao=@Id", connection);
            cmd.Parameters.AddWithValue("@Id", idTransacao);
            cmd.ExecuteNonQuery();

            AtualizarSaldoUsuario(transacao.UsuarioId);
        }

        // ============================================================
        // CONSULTAS (SELECT)
        // ============================================================

        /// <summary>
        /// Busca uma transação específica pelo seu Id.
        /// Retorna null se não existir.
        /// </summary>
        public Transacao GetById(int id)
        {
            using var connection = GetConnection();
            using var cmd = new SqliteCommand("SELECT * FROM Transacoes WHERE IdTransacao=@Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return Map(reader);
            return null;
        }

        /// <summary>
        /// Retorna todas as transações (receitas e despesas) de um usuário, ordenadas da mais recente para a mais antiga.
        /// </summary>
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

        /// <summary>
        /// Retorna apenas as transações de um tipo específico ("Receita" ou "Despesa") para um usuário.
        /// Usado nas telas de despesas e receitas.
        /// </summary>
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

        // ============================================================
        // MAPEAMENTO DE DADOS (SqliteDataReader -> Objeto Transacao)
        // ============================================================

        // Converte a linha atual do leitor em um objeto Transacao.
        // Usa os nomes das colunas (mais robusto que ordinais) e trata DBNull.
        private Transacao Map(SqliteDataReader reader)
        {
            return new Transacao
            {
                IdTransacao = Convert.ToInt32(reader["IdTransacao"]),
                DescricaoTransacao = reader["DescricaoTransacao"]?.ToString() ?? "",
                TipoTransacao = reader["TipoTransacao"]?.ToString() ?? "",
                ValorTransacao = Convert.ToDouble(reader["ValorTransacao"]),
                DataTransacao = DateTime.Parse(reader["DataTransacao"].ToString()),
                Local = reader["OrigemTransacao"]?.ToString(),
                ParcelasTransacao = Convert.ToInt32(reader["ParcelasTransacao"]),
                FormaPagamento = reader["FormaPagamento"]?.ToString() ?? "",
                CondicaoPagamento = reader["CondicaoPagamento"]?.ToString() ?? "",
                UsuarioId = Convert.ToInt32(reader["IdUsuario"]),
                CategoriaId = reader["IdCategoria"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdCategoria"]),
                CartaoId = reader["IdCartao"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdCartao"])
            };
        }
    }
}