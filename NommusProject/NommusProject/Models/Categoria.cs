namespace NommusProject
{
    public class Categoria
    {
        public string IdCategoria { get; set; }
        public string NomeCategoria { get; set; }
        public string DescricaoCategoria { get; set; }
        public string TipoCategoria { get; set; } = "Despesa";
        public string CorCategoria { get; set; } = "#3498db";
        public bool Ativa { get; set; } = true;
        public bool CategoriaFixa { get; set; } = false;
    }
}