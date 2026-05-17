using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Runtime.ConstrainedExecution;

namespace NommusProject.Data
{
    // Classe estática responsável por inicializar o banco de dados SQLite.
    // Ela cria a pasta do aplicativo (se não existir), as tabelas necessárias
    // e insere dados iniciais (categorias padrão) se a tabela estiver vazia.
    public static class DatabaseInitializer
    {
        // Caminho da pasta onde o banco de dados será armazenado.
        // Usa a pasta de dados do aplicativo do usuário (AppData\Roaming\NommusApp no Windows).
        private static readonly string DbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NommusApp");

        // Caminho completo do arquivo do banco de dados (nommus.db).
        public static string DbPath => Path.Combine(DbFolder, "nommus.db");

        // Método principal: garante que a pasta existe, cria as tabelas se não existirem
        // e insere categorias padrão (apenas na primeira execução).
        public static void Initialize()
        {
            // Cria a pasta do aplicativo se ela não existir (ex: %APPDATA%\NommusApp)
            if (!Directory.Exists(DbFolder))
                Directory.CreateDirectory(DbFolder);

            // Abre (ou cria) a conexão com o banco de dados SQLite
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // Script SQL para criação de todas as tabelas do sistema.
            // Usa "IF NOT EXISTS" para não recriar tabelas já existentes.
            var createTablesSql = @"
            -- Tabela de usuários (autenticação e dados básicos)
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Email TEXT UNIQUE,                  -- Email único (login)
                Telefone TEXT,
                Senha TEXT NOT NULL,
                Tipo INTEGER NOT NULL,               -- 1=Basic, 2=Premium, 3=Adm
                IdAdm INTEGER DEFAULT 0,             -- Flag redundante (0= false, 1=true)
                SaldoDisponivel REAL DEFAULT 0.0
            );

            -- Tabela de categorias (para classificar transações)
            CREATE TABLE IF NOT EXISTS Categorias (
                IdCategoria INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeCategoria TEXT NOT NULL,
                DescricaoCategoria TEXT
            );

            -- Tabela de cartões de crédito/débito (associados a um usuário)
            CREATE TABLE IF NOT EXISTS Cartoes (
                IdCartao INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeCartao TEXT NOT NULL,            -- Nome do banco ou fornecedor
                LimiteCartao REAL,
                DataVencimento TEXT,                 -- Data de vencimento da fatura (formato ISO)
                BandeiraCartao TEXT,                 -- Visa, Mastercard, etc.
                IdUsuario INTEGER NOT NULL,
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id)
            );

            -- Tabela de transações (receitas e despesas) – principal tabela financeira
            CREATE TABLE IF NOT EXISTS Transacoes (
                IdTransacao INTEGER PRIMARY KEY AUTOINCREMENT,
                DescricaoTransacao TEXT,              -- Descrição do gasto/receita
                TipoTransacao TEXT,                   -- 'Receita' ou 'Despesa'
                ValorTransacao REAL,
                DataTransacao TEXT,                   --Data da transação(ISO)
                OrigemTransacao TEXT,                 --Local onde ocorreu(opcional)
                ParcelasTransacao INTEGER DEFAULT 1,
                FormaPagamento TEXT,                  --Débito, Crédito, Dinheiro, Depósito...
                CondicaoPagamento TEXT,               --À vista, parcelado, etc.
                IdUsuario INTEGER NOT NULL,
                IdCategoria INTEGER,                  --Categoria(pode ser nula)
                IdCartao INTEGER,                     --Cartão usado(se for crédito)
                FOREIGN KEY(IdUsuario) REFERENCES Usuarios(Id),
                FOREIGN KEY(IdCategoria) REFERENCES Categorias(IdCategoria),
                FOREIGN KEY(IdCartao) REFERENCES Cartoes(IdCartao)
            );

            --Tabela de metas de economia(objetivos financeiros)
            CREATE TABLE IF NOT EXISTS Metas(
                IdMeta INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeMeta TEXT,
                ValorMeta REAL,                      --Objetivo total
                DataInicial TEXT,                    --Data de início da meta
                DataFinal TEXT,                      --Data prevista para conclusão
                StatusMeta INTEGER DEFAULT 0,        --0 = em andamento, 1 = concluída
                IdUsuario INTEGER NOT NULL,
                ValorAtual REAL DEFAULT 0,           --Valor já economizado
                FOREIGN KEY(IdUsuario) REFERENCES Usuarios(Id)
            );
            ";

            // Executa o script de criação das tabelas
            using var command = new SqliteCommand(createTablesSql, connection);
            command.ExecuteNonQuery();

            // ======================================================
            // MIGRAÇÃO: Adicionar coluna FotoPerfil se não existir
            // ======================================================
            using var checkColumnCmd = new SqliteCommand("PRAGMA table_info(Usuarios)", connection);
            using var reader = checkColumnCmd.ExecuteReader();
            bool hasFotoPerfil = false;
            while (reader.Read())
            {
                if (reader["name"].ToString() == "FotoPerfil")
                {
                    hasFotoPerfil = true;
                    break;
                }
            }
            if (!hasFotoPerfil)
            {
                using var alterCmd = new SqliteCommand("ALTER TABLE Usuarios ADD COLUMN FotoPerfil TEXT", connection);
                alterCmd.ExecuteNonQuery();
            }

            // ======================================================
            // INSERÇÃO DE DADOS INICIAIS (Categorias padrão)
            // ======================================================

            // Verifica se a tabela Categorias já possui algum registro.
            // Se estiver vazia (count == 0), insere as categorias padrão.
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Categorias", connection);
            long count = (long)checkCmd.ExecuteScalar();
            if (count == 0)
            {
                // Script de inserção das 7 categorias básicas recomendadas.
                var insertCat = @"INSERT INTO Categorias (NomeCategoria, DescricaoCategoria) VALUES
                      ('Alimentação', 'Gastos com alimentação'),
                      ('Transporte', 'Ônibus, Uber, gasolina'),
                      ('Moradia', 'Aluguel, condomínio, luz, água'),
                      ('Lazer', 'Cinema, shows, viagens'),
                      ('Saúde', 'Plano de saúde, farmácia'),
                      ('Educação', 'Cursos, livros, mensalidades'),
                      ('Outros', 'Despesas diversas');";
                using var insertCmd = new SqliteCommand(insertCat, connection);
                insertCmd.ExecuteNonQuery();
            }
        }
    }
}