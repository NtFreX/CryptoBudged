using System;

namespace CryptoBudged.Models
{
    public class ExchangeModel
    {
        public Guid Id { get; set; }
        public CurrencyModel OriginCurrency { get; set; }
        public double OriginAmount { get; set; }
        public CurrencyModel TargetCurrency { get; set; }
        public double TargetAmount { get; set; }
        public double ExchangeRate { get; set; }
        public double Fees { get; set; }
        public ExchangePlatformModel ExchangePlatform { get; set; }
        public DateTime DateTime { get; set; }
    }
}