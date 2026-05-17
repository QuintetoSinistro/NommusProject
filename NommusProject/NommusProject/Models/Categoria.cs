using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NommusProject
{
    public class Categoria
    {
        public string IdCategoria { get; set; }
        public string NomeCategoria { get; set; }
        public string DescricaoCategoria { get; set; }
        public string TipoCategoria { get; set; } = "Despesa"; // "Receita" ou "Despesa"
        public string CorCategoria { get; set; } = "#3498db";
        public bool Ativa { get; set; } = true;
        public bool CategoriaFixa { get; set; } = false; // Para despesas fixas como aluguel

        // Métodos de persistência
        public async Task AdicionarCategoriaAsync()
        {
            var categorias = await CarregarCategoriasAsync();

            if (string.IsNullOrEmpty(this.IdCategoria))
            {
                this.IdCategoria = Guid.NewGuid().ToString();
            }

            categorias.Add(this);
            await SalvarCategoriasAsync(categorias);
        }

        public async Task AlterarCategoriaAsync()
        {
            var categorias = await CarregarCategoriasAsync();
            var categoriaExistente = categorias.FirstOrDefault(c => c.IdCategoria == this.IdCategoria);
            if (categoriaExistente != null)
            {
                categorias.Remove(categoriaExistente);
                categorias.Add(this);
                await SalvarCategoriasAsync(categorias);
            }
        }

        public async Task ExcluirCategoriaAsync()
        {
            var categorias = await CarregarCategoriasAsync();
            var categoriaExistente = categorias.FirstOrDefault(c => c.IdCategoria == this.IdCategoria);
            if (categoriaExistente != null)
            {
                categorias.Remove(categoriaExistente);
                await SalvarCategoriasAsync(categorias);
            }
        }

        // Métodos estáticos
        public static async Task<List<Categoria>> CarregarCategoriasAsync()
        {
            return await GerenciadorDados.CarregarAsync<List<Categoria>>("categorias.json");
        }

        public static async Task SalvarCategoriasAsync(List<Categoria> categorias)
        {
            await GerenciadorDados.SalvarAsync(categorias, "categorias.json");
        }

        public static async Task<Categoria> BuscarCategoriaPorIdAsync(string id)
        {
            var categorias = await CarregarCategoriasAsync();
            return categorias.FirstOrDefault(c => c.IdCategoria == id);
        }

        public static async Task<List<Categoria>> CarregarCategoriasPorTipoAsync(string tipo)
        {
            var categorias = await CarregarCategoriasAsync();
            return categorias.Where(c => c.TipoCategoria == tipo && c.Ativa).ToList();
        }

        // Método para carregar categorias pessoais padrão
        public static async Task CarregarCategoriasPadraoAsync()
        {
            var categorias = await CarregarCategoriasAsync();
            if (categorias.Count == 0)
            {
                var categoriasPadrao = new List<Categoria>
                {
                    // RECEITAS
                    new Categoria {
                        IdCategoria = "receita-salario",
                        NomeCategoria = "Salário",
                        DescricaoCategoria = "Rendimento do trabalho",
                        TipoCategoria = "Receita",
                        CorCategoria = "#27ae60",
                        CategoriaFixa = true
                    },
                    new Categoria {
                        IdCategoria = "receita-freelance",
                        NomeCategoria = "Freelance",
                        DescricaoCategoria = "Trabalhos extras",
                        TipoCategoria = "Receita",
                        CorCategoria = "#2ecc71"
                    },
                    new Categoria {
                        IdCategoria = "receita-investimentos",
                        NomeCategoria = "Investimentos",
                        DescricaoCategoria = "Rendimentos de aplicações",
                        TipoCategoria = "Receita",
                        CorCategoria = "#1abc9c"
                    },
                    
                    // DESPESAS FIXAS
                    new Categoria {
                        IdCategoria = "despesa-moradia",
                        NomeCategoria = "Moradia",
                        DescricaoCategoria = "Aluguel, condomínio, IPTU",
                        TipoCategoria = "Despesa",
                        CorCategoria = "#e74c3c",
                        CategoriaFixa = true
                    },
                    new Categoria {
                        IdCategoria = "despesa-transporte",
                        NomeCategoria = "Transporte",
                        DescricaoCategoria = "Combustível, ônibus, Uber",
                        TipoCategoria = "Despesa",
                        CorCategoria = "#e67e22",
                        CategoriaFixa = true
                    },
                    new Categoria {
                        IdCategoria = "despesa-alimentacao",
                        NomeCategoria = "Alimentação",
                        DescricaoCategoria = "Mercado, restaurantes",
                        TipoCategoria = "Despesa",
                        CorCategoria = "#d35400",
                        CategoriaFixa = true
                    },
                    
                    // DESPESAS VARIÁVEIS
                    new Categoria {
                        IdCategoria = "despesa-lazer",
                        NomeCategoria = "Lazer",
                        DescricaoCategoria = "Cinema, passeios, hobbies",
                        TipoCategoria = "Despesa",
                        CorCategoria = "#9b59b6"
                    },
                    new Categoria {
                        IdCategoria = "despesa-saude",
                        NomeCategoria = "Saúde",
                        DescricaoCategoria = "Plano de saúde, medicamentos",
                        TipoCategoria = "Despesa",
                        CorCategoria = "#3498db"
                    },
                    new Categoria {
                        IdCategoria = "despesa-educacao",
                        NomeCategoria = "Educação",
                        DescricaoCategoria = "Cursos, livros, faculdade",
                        TipoCategoria = "Despesa",
                        CorCategoria = "#f1c40f"
                    }
                };

                await SalvarCategoriasAsync(categoriasPadrao);
            }
        }
    }
}
