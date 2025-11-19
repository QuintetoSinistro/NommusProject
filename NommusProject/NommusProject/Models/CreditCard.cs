namespace Nommus
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
        public System.Windows.Media.Brush CardColor { get; set; }
    }
}