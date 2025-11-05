using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NommusProject
{
    public class Metas
    {
        public int IdMeta { get; set; }
        public string NomeMeta { get; set; }
        public double ValorMeta { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
        public bool StatusMeta { get; set; }

        // Métodos de persistência
        public async Task DefinirMetaAsync()
        {
            var metas = await CarregarMetasAsync();
            this.IdMeta = metas.Count > 0 ? metas.Max(m => m.IdMeta) + 1 : 1;
            this.StatusMeta = false; // Meta nova inicia como não concluída
            metas.Add(this);
            await SalvarMetasAsync(metas);
        }

        public async Task AlterarMetaAsync()
        {
            var metas = await CarregarMetasAsync();
            var metaExistente = metas.FirstOrDefault(m => m.IdMeta == this.IdMeta);
            if (metaExistente != null)
            {
                metas.Remove(metaExistente);
                metas.Add(this);
                await SalvarMetasAsync(metas);
            }
        }

        public async Task ExcluirMetaAsync()
        {
            var metas = await CarregarMetasAsync();
            var metaExistente = metas.FirstOrDefault(m => m.IdMeta == this.IdMeta);
            if (metaExistente != null)
            {
                metas.Remove(metaExistente);
                await SalvarMetasAsync(metas);
            }
        }

        // Métodos estáticos
        public static async Task<List<Metas>> CarregarMetasAsync()
        {
            return await GerenciadorDados.CarregarAsync<List<Metas>>("metas.json");
        }

        public static async Task SalvarMetasAsync(List<Metas> metas)
        {
            await GerenciadorDados.SalvarAsync(metas, "metas.json");
        }

        // Método para verificar metas concluídas
        public static async Task<List<Metas>> CarregarMetasConcluidasAsync()
        {
            var metas = await CarregarMetasAsync();
            return metas.Where(m => m.StatusMeta).ToList();
        }
    }
}