using Microsoft.Data.Sqlite;
using System;
using System.Diagnostics;

namespace NommusProject.Tools
{
    public static class PragmaTableInfoRunner
    {
        // Chame este método da Janela Immediate:
        // NommusProject.Tools.PragmaTableInfoRunner.Run();
        public static void Run()
        {
            var dbPath = DatabaseInitializer.DbPath;
            using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            using var cmd = new SqliteCommand("PRAGMA table_info('Transacoes');", conn);
            using var reader = cmd.ExecuteReader();

            Debug.WriteLine("cid\tname\ttype\tnotnull\tdflt_value\tpk");
            while (reader.Read())
            {
                var cid = reader.GetInt32(0);
                var name = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
                var type = reader.IsDBNull(2) ? "NULL" : reader.GetString(2);
                var notnull = reader.GetInt32(3);
                var dflt = reader.IsDBNull(4) ? "NULL" : reader.GetValue(4)?.ToString();
                var pk = reader.GetInt32(5);

                Debug.WriteLine($"{cid}\t{name}\t{type}\t{notnull}\t{dflt}\t{pk}");
            }
        }
    }
}