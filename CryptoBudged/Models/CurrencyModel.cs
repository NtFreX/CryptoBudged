namespace CryptoBudged.Models
{
    public class CurrencyModel
    {
        public string ShortName { get; set; }
        public string DisplayName { get; set; }
        public bool IsCryptoCurrency { get; set; }

        public override string ToString()
            => $"{DisplayName} ({ShortName})";
    }
}