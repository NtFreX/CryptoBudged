using System.Threading.Tasks;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.Services
{
    public class CryptoCurrencyService
    {
        public async Task<PriceModel> GetPriceOfCurrencyAsync(CurrencyModel currency)
        {
            var content = await HttpCachingService.Instance.GetStringAsync($"https://min-api.cryptocompare.com/data/price?fsym={currency.ShortName}&tsyms=BTC,USD,EUR,CHF,ETH");
            return JsonConvert.DeserializeObject<PriceModel>(content);
        }
    }
}
