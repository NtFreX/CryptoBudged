using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Extensions;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi.Wrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NtFreX.RestClient.NET.Flow;

namespace CryptoBudged.ThirdPartyApi
{
    public class BitfinexApi : IDisposable
    {
        private readonly BitfinexApiWrapper _bitfinexApiWrapper = new BitfinexApiWrapper();

        private BitfinexApi()
        {
            GetExchangeSymbols = new AsyncCachedFunction<List<string>>(GetExchangeSymbolsAsync, TimeSpan.FromMinutes(10));
            GetExchangeRate = new AsyncTimeRateLimitedFunction<CurrencyModel, CurrencyModel, DateTime, double>(GetNearestExchangeRateAsync, _bitfinexApiWrapper.RestClient.MinInterval[BitfinexApiWrapper.BitfinexApiEndpointNames.Trades], DoNotRateLimitGetExchangeRateAsync);

            _getSupportedSymbol = new AsyncCachedFunction<CurrencyModel, CurrencyModel, string>(GetSupportedSymbolAsync, TimeSpan.FromMinutes(10));
        }

        #region Public
        public readonly AsyncCachedFunction<List<string>> GetExchangeSymbols;
        private async Task<List<string>> GetExchangeSymbolsAsync()
        {
            var response = await _bitfinexApiWrapper.RestClient.CallEndpointAsync(BitfinexApiWrapper.BitfinexApiEndpointNames.Symbols);
            return JsonConvert.DeserializeObject<List<string>>(response);
        }

        public AsyncTimeRateLimitedFunction<CurrencyModel, CurrencyModel, DateTime, double> GetExchangeRate;
        private async Task<double> GetNearestExchangeRateAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var exchangeRateSymbol = await _getSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency);
            var searchInMiliseconds = dateTime.ToUnixTimeMilliseconds();
            var end = GetEndDateTimeForApiCall(dateTime);

            var maxTradeOffset = 3600000;
            var lastTradeTime = 0L;
            var lastTradePrice = 0.0;
            var trades = await _bitfinexApiWrapper.RestClient.CallEndpointAsync(BitfinexApiWrapper.BitfinexApiEndpointNames.Trades, exchangeRateSymbol, end);
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
        private DateTime GetEndDateTimeForApiCall(DateTime dateTime)
            => dateTime.AddMinutes(4);
        private async Task<bool> DoNotRateLimitGetExchangeRateAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var end = GetEndDateTimeForApiCall(dateTime);
            var symbol = await _getSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency);
            return _bitfinexApiWrapper.RestClient.IsCached(BitfinexApiWrapper.BitfinexApiEndpointNames.Trades, symbol, end);
        }

        public async Task<bool> DoExchangeRatesExist(CurrencyModel originCurrency, CurrencyModel targetCurrency)
            => !string.IsNullOrEmpty(await _getSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency));
        #endregion

        #region Private
        private readonly AsyncCachedFunction<CurrencyModel, CurrencyModel, string> _getSupportedSymbol;
        private async Task<string> GetSupportedSymbolAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
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

        public void Dispose()
        {
            _bitfinexApiWrapper?.Dispose();
        }
    }
}
