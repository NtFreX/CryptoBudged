using System;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi.Wrapper;
using Newtonsoft.Json.Linq;

namespace CryptoBudged.ThirdPartyApi
{
    public class FixerApi : IDisposable
    {
        private readonly FixerApiWrapper _fixerApiWrapper = new FixerApiWrapper();

        private FixerApi() { }

        public async Task<double> GetExchangeRateAsync(CurrencyModel baseCurrency, CurrencyModel targetCurrency,DateTime dateTime)
        {
            var content = await _fixerApiWrapper.RestClient.CallEndpointAsync(FixerApiWrapper.FixerApiEndpointNames.Trades, dateTime, baseCurrency);
            dynamic obj = JObject.Parse(content);
            if (!(obj.rates is JObject rates))
                throw new Exception();
            return double.Parse(rates.Property(targetCurrency.ShortName).Value.ToString());
        } 
        public async Task<TimeSpan> IsGetExchangeRateRateLimitedAsync(CurrencyModel baseCurrency, DateTime dateTime)
            => await _fixerApiWrapper.RestClient.IsRateLimitedAsync(FixerApiWrapper.FixerApiEndpointNames.Trades, dateTime, baseCurrency);
        
        public static FixerApi Instance { get; } = new FixerApi();

        public void Dispose()
        {
            _fixerApiWrapper?.Dispose();
        }
    }
}
