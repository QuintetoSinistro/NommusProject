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
                Tipo INTEGER NOT NULL,
                IdAdm INTEGER DEFAULT 0,
                SaldoDisponivel REAL DEFAULT 0.0
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
        }
    }
}