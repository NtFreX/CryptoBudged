using System;

namespace CryptoBudged.Models
{
    public class DepositWithdrawlModel
    {
        public Guid Id { get; set; }
        public CurrencyModel Currency { get; set; }
        public ExchangePlatformModel OriginPlatform { get; set; }
        public string OriginAdress { get; set; }
        public ExchangePlatformModel TargetPlatform { get; set; }
        public string TargetAdress { get; set; }
        public double Amount { get; set; }
        public double Fees { get; set; }
        public DateTime DateTime { get; set; }
        public bool WithDrawFromHoldings { get; set; }
    }
}
