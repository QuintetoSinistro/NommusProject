namespace NommusProject
{
    public class Transacao
    {
        public int IdTransacao { get; set; }
        public string DescricaoTransacao { get; set; }
        public string TipoTransacao { get; set; } // "Receita" ou "Despesa"
        public double ValorTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public string Local { get; set; }
        public int ParcelasTransacao { get; set; } = 1;
        public string FormaPagamento { get; set; }
        public string CondicaoPagamento { get; set; } = "À vista";
        public string Observacao { get; set; } = string.Empty;

        // Relacionamentos
        public int UsuarioId { get; set; }
        public string CategoriaId { get; set; }
        public int? CartaoId { get; set; }
        public bool TransacaoFixa { get; set; } = false;
    }
}
