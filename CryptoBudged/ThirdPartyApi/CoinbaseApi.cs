using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoBudged.Helpers;
using CryptoBudged.Models;
using Newtonsoft.Json.Linq;

namespace CryptoBudged.ThirdPartyApi
{
    public class CoinbaseApi
    {
        private static readonly HttpClient CoinbaseHttpClient = new HttpClient();

        #region Raw data
        private static readonly Func<string> GetCurrenciesUriBuilder = () => "https://api.coinbase.com/v2/currencies";
        private static readonly Func<CurrencyModel, CurrencyModel, DateTime, string> GetSpotPriceUriBuilder = (originCurrency, targetCurrency, dateTime) => $"https://api.coinbase.com/v2/prices/{originCurrency.ShortName}-{targetCurrency.ShortName}/spot?date={FormatDate(dateTime)}";

        private static readonly AdvancedHttpRequest GetCurrenciesRequest = new AdvancedHttpRequest(httpClient: CoinbaseHttpClient, uriBuilder: GetCurrenciesUriBuilder, maxInterval: TimeSpan.FromSeconds(5), cachingTime: TimeSpan.FromDays(1), maxRetries: 3, replayOnStatusCode: new [] { 500, 520 });
        private static readonly AdvancedHttpRequest<CurrencyModel, CurrencyModel, DateTime> GetSpotPriceRequest = new AdvancedHttpRequest<CurrencyModel, CurrencyModel, DateTime>(httpClient: CoinbaseHttpClient, uriBuilder: GetSpotPriceUriBuilder, maxInterval: TimeSpan.FromSeconds(4), cachingTime: TimeSpan.MaxValue, maxRetries: 3, replayOnStatusCode: new [] { 500, 520 });
        #endregion

        private CoinbaseApi() { }

        #region Public
        public readonly AsyncCachedFunction<List<string>> GetCurrencies = new AsyncCachedFunction<List<string>>(GetCurrenciesAsync, TimeSpan.FromMinutes(10));
        private static async Task<List<string>> GetCurrenciesAsync()
        {
            var currencies = await GetCurrenciesRequest.ExecuteAsync();
            var obj = JObject.Parse(currencies);
            return obj.Value<JArray>("data").Select(x => x.Value<string>("id")).ToList();
        }

        public async Task<double> GetSpotPriceAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var price = await GetSpotPriceRequest.ExecuteAsync(originCurrency, targetCurrency, dateTime);
            var obj = JObject.Parse(price);
            return obj.Value<JObject>("data").Value<double>("amount");
        }
        public async Task<TimeSpan> IsGetSpotPriceRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
            => await GetSpotPriceRequest.IsRateLimitedAsync(originCurrency, targetCurrency, dateTime);
        #endregion
        
        #region Helpers
        private static string FormatDate(DateTime dateTime)
            => $"{dateTime.Year:0000}-{dateTime.Month:00}-{dateTime.Day:00}";
        #endregion

        public static CoinbaseApi Instance { get; } = new CoinbaseApi();
    }
}
