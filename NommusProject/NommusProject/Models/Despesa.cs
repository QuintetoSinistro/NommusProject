namespace NommusProject
{
    public class Despesa : Transacao
    {
        public bool DespesaEssencial { get; set; } = true;
        public bool DespesaRecorrente { get; set; } = false;

        public Despesa()
        {
            this.TipoTransacao = "Despesa";
            this.FormaPagamento = "Dinheiro";
        }
    }
}