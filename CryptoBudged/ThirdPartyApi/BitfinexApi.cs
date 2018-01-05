using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoBudged.Extensions;
using CryptoBudged.Helpers;
using CryptoBudged.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoBudged.ThirdPartyApi
{
    public class BitfinexApi
    {
        private static readonly HttpClient BitfinexHttpClient = new HttpClient();

        #region Raw data
        private static readonly Func<string> GetExchangeSymbolsUriBuilder = () => "https://api.bitfinex.com/v1/symbols";
        private static readonly Func<string, DateTime, string> GetTradesUriBuilder = (symbol, end) => $"https://api.bitfinex.com/v2/trades/t{symbol}/hist?end={end.ToUnixTimeMilliseconds()}";

        private static readonly AdvancedHttpRequest GetExchangeSymbolsRequest = new AdvancedHttpRequest(httpClient: BitfinexHttpClient, uriBuilder: GetExchangeSymbolsUriBuilder, maxInterval: TimeSpan.FromSeconds(5), cachingTime: TimeSpan.FromDays(1), maxRetries: 3, replayOnStatusCode: new [] { 500, 520 });
        private static readonly AdvancedHttpRequest<string, DateTime> GetTradesRequest = new AdvancedHttpRequest<string, DateTime>(httpClient: BitfinexHttpClient, uriBuilder: GetTradesUriBuilder, maxInterval: TimeSpan.FromSeconds(4), cachingTime: TimeSpan.MaxValue, maxRetries: 3, replayOnStatusCode: new [] { 500, 520 });

        #endregion
        private BitfinexApi() { }

        #region Public
        public readonly AsyncCachedFunction<List<string>> GetExchangeSymbols = new AsyncCachedFunction<List<string>>(GetExchangeSymbolsAsync, TimeSpan.FromMinutes(10));
        private static async Task<List<string>> GetExchangeSymbolsAsync()
        {
            var response = await GetExchangeSymbolsRequest.ExecuteAsync();
            return JsonConvert.DeserializeObject<List<string>>(response);
        }

        public AsyncRateLimitedFunction<CurrencyModel, CurrencyModel, DateTime, double> GetExchangeRate = new AsyncRateLimitedFunction<CurrencyModel, CurrencyModel, DateTime, double>(GetNearestExchangeRateAsync, GetTradesRequest.MaxInterval, DoNotRateLimitGetExchangeRateAsync);
        private static async Task<double> GetNearestExchangeRateAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var exchangeRateSymbol = await GetSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency);
            var searchInMiliseconds = dateTime.ToUnixTimeMilliseconds();
            var end = GetEndDateTimeForApiCall(dateTime);

            var maxTradeOffset = 3600000;
            var lastTradeTime = 0L;
            var lastTradePrice = 0.0;
            var trades = await GetTradesRequest.ExecuteAsync(exchangeRateSymbol, end);
            foreach (var trade in JArray.Parse(trades))
            {
                var items = trade as JArray;
                if (items == null)
                    continue;

                var tradeTime = items[1].Value<long>();
                var tradePrice = items[3].Value<double>();

                if (tradeTime > searchInMiliseconds)
                {
                    var tradeDifference = tradeTime - searchInMiliseconds;
                    var lastTradeDifference = (lastTradeTime - searchInMiliseconds) * -1;
                    if (tradeDifference > lastTradeDifference)
                    {
                        if (lastTradeDifference > maxTradeOffset)
                            throw new Exception($"The found exchange rate for is `{lastTradeDifference}` miliseconds older then the searched one.");
                        return lastTradePrice;
                    }

                    if (tradeDifference > maxTradeOffset)
                        throw new Exception($"The found exchange rate for is `{tradeDifference}` miliseconds newer then the searched one.");
                    if (tradeDifference < lastTradeDifference)
                    {
                        return tradePrice;
                    }
                    return tradePrice;
                }

                lastTradeTime = tradeTime;
                lastTradePrice = tradePrice;
            }

            if (lastTradeTime == 0)
                throw new Exception($"No exchange rate for `{exchangeRateSymbol}` on `{dateTime}` found.");

            var lastTradeOffset = (lastTradeTime - searchInMiliseconds) * -1;
            if (lastTradeOffset >= maxTradeOffset)
                throw new Exception($"The found exchange rate for is `{lastTradeOffset}` miliseconds older then the searched one.");

            return lastTradePrice;
        }
        private static DateTime GetEndDateTimeForApiCall(DateTime dateTime)
            => dateTime.AddMinutes(4);
        private static async Task<bool> DoNotRateLimitGetExchangeRateAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var end = GetEndDateTimeForApiCall(dateTime);
            var symbol = await GetSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency);
            return GetTradesRequest.IsCached(symbol, end);
        }

        public async Task<bool> DoExchangeRatesExist(CurrencyModel originCurrency, CurrencyModel targetCurrency)
            => !string.IsNullOrEmpty(await GetSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency));
        #endregion

        #region Private
        private static readonly AsyncCachedFunction<CurrencyModel, CurrencyModel, string> GetSupportedSymbol = new AsyncCachedFunction<CurrencyModel, CurrencyModel, string>(GetSupportedSymbolAsync, TimeSpan.FromMinutes(10));
        private static async Task<string> GetSupportedSymbolAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
        {
            var exchangeRateSymbols = await Instance.GetExchangeSymbols.ExecuteAsync();
            exchangeRateSymbols = exchangeRateSymbols.Select(x => x.ToUpper()).ToList();

            foreach (var originCurrencySymbol in originCurrency.TradingSymbols)
            {
                foreach (var targetCurrencySymbol in targetCurrency.TradingSymbols)
                {
                    if (exchangeRateSymbols.Contains(originCurrencySymbol + targetCurrencySymbol))
                    {
                        return originCurrencySymbol + targetCurrencySymbol;
                    }
                    if (exchangeRateSymbols.Contains(targetCurrencySymbol + originCurrencySymbol))
                    {
                        return targetCurrencySymbol + originCurrency;
                    }
                }
            }
            return null;
        }
        #endregion

        public static BitfinexApi Instance { get; } = new BitfinexApi();
    }
}
