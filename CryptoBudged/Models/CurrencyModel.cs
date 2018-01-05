using System;
using System.Collections.Generic;

namespace CryptoBudged.Models
{
    public class CurrencyModel : IComparable
    {
        public string ShortName { get; set; }
        public string DisplayName { get; set; }
        public bool IsCryptoCurrency { get; set; }
        public string[] TradingSymbols { get; set; }

        public string ImagePath => $"https://files.coinmarketcap.com/static/img/coins/32x32/{DisplayName.ToLower().Replace(" ", "-")}.png";

        public override string ToString()
            => $"{DisplayName} ({ShortName})";

        public int CompareTo(object obj)
        {
            if (!(obj is CurrencyModel currency))
                return String.CompareOrdinal(null, ShortName);

            return String.CompareOrdinal(currency?.ShortName, ShortName);
        }
    }

    public class CurrencyModelEqualityComparer : IEqualityComparer<CurrencyModel>
    {
        public bool Equals(CurrencyModel x, CurrencyModel y)
            => string.Equals(x?.ShortName, y?.ShortName);

        public int GetHashCode(CurrencyModel obj)
            => obj.ShortName.GetHashCode();
    }
}