using System;

namespace CryptoBudged.Models
{
    public class ExchangePlatformModel : IComparable
    {
        public string DisplayName { get; set; }
        public string Uri { get; set; }
        public bool ExchangesCryptoCurrencies { get; set; }

        public override string ToString()
            => DisplayName;

        public int CompareTo(object obj)
        {
            if (!(obj is ExchangePlatformModel exchangePlatform))
                return string.CompareOrdinal(null, DisplayName);

            return string.CompareOrdinal(exchangePlatform?.DisplayName, DisplayName);
        }
    }
}