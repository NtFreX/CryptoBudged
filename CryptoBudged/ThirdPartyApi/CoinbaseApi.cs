using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi.Wrapper;
using Newtonsoft.Json.Linq;
using NtFreX.RestClient.NET.Flow;

namespace CryptoBudged.ThirdPartyApi
{
    public class CoinbaseApi : IDisposable
    {
        private readonly CoinbaseApiWrapper _coinbaseApiWrapper = new CoinbaseApiWrapper();

        private CoinbaseApi()
        {
            GetCurrencies = new AsyncCachedFunction<List<string>>(GetCurrenciesAsync, TimeSpan.FromMinutes(10));
        }

        #region Public

        public readonly AsyncCachedFunction<List<string>> GetCurrencies;
        private async Task<List<string>> GetCurrenciesAsync()
        {
            var currencies = await _coinbaseApiWrapper.RestClient.CallEndpointAsync(CoinbaseApiWrapper.CoinbaseApiEndpointNames.Currencies);
            var obj = JObject.Parse(currencies);
            return obj.Value<JArray>("data").Select(x => x.Value<string>("id")).ToList();
        }

        public async Task<double> GetSpotPriceAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var price = await _coinbaseApiWrapper.RestClient.CallEndpointAsync(CoinbaseApiWrapper.CoinbaseApiEndpointNames.Prices, originCurrency, targetCurrency, dateTime);
            var obj = JObject.Parse(price);
            return obj.Value<JObject>("data").Value<double>("amount");
        }
        public async Task<TimeSpan> IsGetSpotPriceRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
            => await _coinbaseApiWrapper.RestClient.IsRateLimitedAsync(CoinbaseApiWrapper.CoinbaseApiEndpointNames.Prices, originCurrency, targetCurrency, dateTime);
        #endregion
        
        public static CoinbaseApi Instance { get; } = new CoinbaseApi();

        public void Dispose()
        {
            _coinbaseApiWrapper?.Dispose();
        }
    }
}
