namespace NommusProject
{
    public class Cartao
    {
        public int IdCartao { get; set; }
        public string NomeCartao { get; set; }
        public double LimiteCartao { get; set; }
        public DateTime DataVencimento { get; set; }
        public string BandeiraCartao { get; set; }
    }
}