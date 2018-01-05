using System;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoBudged.Helpers;
using CryptoBudged.Models;
using Newtonsoft.Json.Linq;

namespace CryptoBudged.ThirdPartyApi
{
    public class FixerApi
    {
        private static readonly HttpClient FixerHttpClient = new HttpClient();

        #region Raw data
        private static readonly Func<CurrencyModel, DateTime, string> GetTradesUriBUilder = (baseCurrency, dateTime) => $"https://api.fixer.io/{FormatDate(dateTime)}?base={baseCurrency.ShortName}";

        private static readonly AdvancedHttpRequest<CurrencyModel, DateTime> GetTradesRequest = new AdvancedHttpRequest<CurrencyModel, DateTime>(httpClient: FixerHttpClient, uriBuilder: GetTradesUriBUilder, maxInterval: TimeSpan.FromSeconds(4), cachingTime: TimeSpan.MaxValue, maxRetries: 3, replayOnStatusCode: new [] { 500, 520 });
        #endregion

        private FixerApi() { }

        public async Task<double> GetExchangeRateAsync(CurrencyModel baseCurrency, CurrencyModel targetCurrency,DateTime dateTime)
        {
            var content = await GetTradesRequest.ExecuteAsync(baseCurrency, dateTime);
            dynamic obj = JObject.Parse(content);
            if (!(obj.rates is JObject rates))
                throw new Exception();
            return double.Parse(rates.Property(targetCurrency.ShortName).Value.ToString());
        } 
        public async Task<TimeSpan> IsGetExchangeRateRateLimitedAsync(CurrencyModel baseCurrency, DateTime dateTime)
            => await GetTradesRequest.IsRateLimitedAsync(baseCurrency, dateTime);

        #region Helpers
        private static string FormatDate(DateTime dateTime)
            => $"{dateTime.Year:0000}-{dateTime.Month:00}-{dateTime.Day:00}";
        #endregion

        public static FixerApi Instance { get; } = new FixerApi();
    }
}
