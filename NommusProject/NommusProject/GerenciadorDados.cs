using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NommusProject
{
    public static class GerenciadorDados
    {
        private static readonly string AppDataPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "NommusApp");

        static GerenciadorDados()
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public static async Task SalvarAsync<T>(T objeto, string nomeArquivo)
        {
            try
            {
                string caminhoCompleto = Path.Combine(AppDataPath, nomeArquivo);
                string json = JsonSerializer.Serialize(objeto, GetJsonOptions());
                await File.WriteAllTextAsync(caminhoCompleto, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar {nomeArquivo}: {ex.Message}", ex);
            }
        }

        public static async Task<T> CarregarAsync<T>(string nomeArquivo) where T : new()
        {
            try
            {
                string caminhoCompleto = Path.Combine(AppDataPath, nomeArquivo);

                if (!File.Exists(caminhoCompleto))
                    return new T();

                string json = await File.ReadAllTextAsync(caminhoCompleto);
                return JsonSerializer.Deserialize<T>(json, GetJsonOptions()) ?? new T();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao carregar {nomeArquivo}: {ex.Message}", ex);
            }
        }
    }
}