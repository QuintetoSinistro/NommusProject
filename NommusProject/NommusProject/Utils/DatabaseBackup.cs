using NommusProject.Data;
using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject.Utils
{
    public static class DatabaseBackup
    {
        private static readonly string DbPath = DatabaseInitializer.DbPath;
        private static readonly string BackupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NommusApp", "Backups");

        public static void CriarBackup()
        {
            try
            {
                if (!Directory.Exists(BackupFolder))
                    Directory.CreateDirectory(BackupFolder);

                string backupName = $"nommus_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                string backupPath = Path.Combine(BackupFolder, backupName);
                File.Copy(DbPath, backupPath, overwrite: true);

                // Opcional: manter apenas os últimos 10 backups
                var files = new DirectoryInfo(BackupFolder).GetFiles("*.db")
                            .OrderByDescending(f => f.CreationTime).Skip(10);
                foreach (var old in files) old.Delete();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Falha no backup: {ex.Message}");
            }
        }
    }
}