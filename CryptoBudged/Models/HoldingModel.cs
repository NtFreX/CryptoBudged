namespace CryptoBudged.Models
{
    public class HoldingModel
    {
        public CurrencyModel Currency { get; set; }
        public double Amount { get; set; }
        public double AmountInChf { get; set; }
        public double AmountInBtc { get; set; }
        public double AmountInEth { get; set; }
        public double PriceInChf { get; set; }
        public double PriceInBtc { get; set; }
        public double PriceInEth { get; set; }
        public double ProfitInChf { get; set; }
        public double InvestmentInChf { get; set; }
        public double ProfitInPercent { get; set; }
    }
}
