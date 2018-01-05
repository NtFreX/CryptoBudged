using System;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoBudged.Helpers;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.ThirdPartyApi
{
    public class CryptocompareApi
    {
        private static readonly HttpClient CryptocompareHttpClient = new HttpClient();

        #region Raw data
        private static readonly Func<CurrencyModel, string> GetCurrentPricesUriBuilder = currency => $"https://min-api.cryptocompare.com/data/price?fsym={currency.ShortName}&tsyms=BTC,USD,EUR,CHF,ETH";

        private static readonly AdvancedHttpRequest<CurrencyModel> GetCurrentPricesRequest = new AdvancedHttpRequest<CurrencyModel>(httpClient: CryptocompareHttpClient, uriBuilder: GetCurrentPricesUriBuilder, maxInterval: TimeSpan.FromSeconds(1), cachingTime: TimeSpan.FromSeconds(5), maxRetries: 3, replayOnStatusCode: new [] { 500, 520 });
        #endregion

        private CryptocompareApi() { }

        #region Public
        public readonly AsyncCachedFunction<CurrencyModel, PriceModel> GetCurrentPrices = new AsyncCachedFunction<CurrencyModel, PriceModel>(GetCurrentPricesAsync, TimeSpan.FromMinutes(10));
        private static async Task<PriceModel> GetCurrentPricesAsync(CurrencyModel currency)
        {
            var content = await GetCurrentPricesRequest.ExecuteAsync(currency);
            return JsonConvert.DeserializeObject<PriceModel>(content);
        }
        #endregion

        public static CryptocompareApi Instance { get; } = new CryptocompareApi();
    }
}
