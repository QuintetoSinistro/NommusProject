using Microsoft.Data.Sqlite;
using System.IO;

namespace NommusProject.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string DbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NommusApp");

        public static string DbPath => Path.Combine(DbFolder, "nommus.db");

        public static void Initialize()
        {
            if (!Directory.Exists(DbFolder))
                Directory.CreateDirectory(DbFolder);

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var createTablesSql = @"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Email TEXT UNIQUE,
                Telefone TEXT,
                Senha TEXT NOT NULL,
<<<<<<< Updated upstream
                Tipo INTEGER NOT NULL,
                IdAdm INTEGER DEFAULT 0,
                SaldoDisponivel REAL DEFAULT 0.0
=======
                Tipo INTEGER NOT NULL,               -- 1=Basic, 2=Premium, 3=Adm
                IdAdm INTEGER DEFAULT 0,             -- Flag redundante (0= false, 1=true)
                SaldoDisponivel REAL DEFAULT 0.0,
                FotoPerfil TEXT
>>>>>>> Stashed changes
            );

            CREATE TABLE IF NOT EXISTS Categorias (
                IdCategoria INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeCategoria TEXT NOT NULL,
                DescricaoCategoria TEXT
            );

            CREATE TABLE IF NOT EXISTS Cartoes (
                IdCartao INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeCartao TEXT NOT NULL,
                LimiteCartao REAL,
                DataVencimento TEXT,
                BandeiraCartao TEXT
            );

            CREATE TABLE IF NOT EXISTS Transacoes (
                IdTransacao INTEGER PRIMARY KEY AUTOINCREMENT,
                DescricaoTransacao TEXT,
                TipoTransacao TEXT,
                ValorTransacao REAL,
                DataTransacao TEXT,
                OrigemTransacao TEXT,
                ParcelasTransacao INTEGER DEFAULT 1,
                FormaPagamento TEXT,
                CondicaoPagamento TEXT,
                IdUsuario INTEGER NOT NULL,
                IdCategoria TEXT,
                IdCartao INTEGER,
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
                FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria),
                FOREIGN KEY (IdCartao) REFERENCES Cartoes(IdCartao)
            );

            CREATE TABLE IF NOT EXISTS DepositosEntrada (
                IdDeposito INTEGER PRIMARY KEY AUTOINCREMENT,
                ValorDeposito REAL,
                DataDeposito TEXT,
                DescricaoDeposito TEXT,
                FormaDeposito TEXT,
                IdUsuario INTEGER NOT NULL,
                NumeroCartao TEXT,
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id)
            );

            CREATE TABLE IF NOT EXISTS SaquesSaida (
                IdSaque INTEGER PRIMARY KEY AUTOINCREMENT,
                ValorSaque REAL,
                DataSaque TEXT,
                DescricaoSaque TEXT,
                FormaDeSaque TEXT,
                IdUsuario INTEGER NOT NULL,
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id)
            );

            CREATE TABLE IF NOT EXISTS Metas (
                IdMeta INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeMeta TEXT,
                ValorMeta REAL,
                DataInicial TEXT,
                DataFinal TEXT,
                StatusMeta INTEGER DEFAULT 0,
                IdUsuario INTEGER NOT NULL,
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id)
            );
            ";

            using var command = new SqliteCommand(createTablesSql, connection);
            command.ExecuteNonQuery();
<<<<<<< Updated upstream
=======

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
            // MIGRAÇÃO: Adicionar coluna NumeroCartao se não existir
            // ======================================================
            using var checkNumCmd = new SqliteCommand("PRAGMA table_info(Cartoes)", connection);
            using var reader2 = checkNumCmd.ExecuteReader();
            bool hasNumeroCartao = false;
            while (reader2.Read())
            {
                if (reader2["name"].ToString() == "NumeroCartao")
                {
                    hasNumeroCartao = true;
                    break;
                }
            }
            if (!hasNumeroCartao)
            {
                using var alterCmd2 = new SqliteCommand("ALTER TABLE Cartoes ADD COLUMN NumeroCartao TEXT", connection);
                alterCmd2.ExecuteNonQuery();
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
>>>>>>> Stashed changes
        }
    }
}