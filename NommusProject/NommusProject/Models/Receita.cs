namespace NommusProject
{
    public class Receita : Transacao
    {
        public string FonteReceita { get; set; } = string.Empty;
        public bool ReceitaRecorrente { get; set; } = false;

        public Receita()
        {
            this.TipoTransacao = "Receita";
            this.FormaPagamento = "Depósito";
        }
    }
}