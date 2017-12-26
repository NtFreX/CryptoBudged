namespace CryptoBudged.Models
{
    public class ExchangePlatformModel
    {
        public string DisplayName { get; set; }
        public string Uri { get; set; }
        public bool ExchangesCryptoCurrencies { get; set; }

        public override string ToString()
            => DisplayName;
    }
}