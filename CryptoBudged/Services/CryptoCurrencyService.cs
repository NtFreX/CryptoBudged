using System.Net.Http;
using System.Threading.Tasks;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.Services
{
    public class CryptoCurrencyService
    {
        public async Task<PriceModel> GetPriceOfCurrencyAsync(CurrencyModel currency)
        {
            using (var client = new HttpClient())
            {
                var content = await client.GetStringAsync($"https://min-api.cryptocompare.com/data/price?fsym={currency.ShortName}&tsyms=BTC,USD,EUR,CHF,ETH");
                return JsonConvert.DeserializeObject<PriceModel>(content);
            }
        }
    }

    public class PriceModel
    {
        public double BTC { get; set; }
        public double USD { get; set; }
        public double EUR { get; set; }
        public double CHF { get; set; }
        public double ETH { get; set; }
    }
}
