using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Extensions;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi.Wrapper;
using Newtonsoft.Json.Linq;
using NtFreX.RestClient.NET.Flow;

namespace CryptoBudged.ThirdPartyApi
{
    public class BinanceApi : IDisposable
    {
        private readonly BinanceApiWrapper _binanceApiWrapper = new BinanceApiWrapper("", "");

        private BinanceApi()
        {
            GetExchangeSymbols = new AsyncCachedFunction<List<string>>(GetExchangeSymbolsAsync, TimeSpan.FromMinutes(10));
            GetExchangeRate = new AsyncTimeRateLimitedFunction<CurrencyModel, CurrencyModel, DateTime, double>(GetExchangeRateAsync, _binanceApiWrapper.RestClient.MinInterval[BinanceApiWrapper.BinanceApiEndpointNames.AggregatedTrades], DoNotRateLimitGetExchangeRateAsync);

            _getSupportedSymbol = new AsyncCachedFunction<CurrencyModel, CurrencyModel, string>(GetSupportedSymbolAsync, TimeSpan.FromMinutes(10));
        }

        #region Public
        public readonly AsyncTimeRateLimitedFunction<CurrencyModel, CurrencyModel, DateTime, double> GetExchangeRate;
        private async Task<double> GetExchangeRateAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var startAndEnd = GetStartAndEndForGetExchangeRate(dateTime);
            var symbol = await _getSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency);
            var searchInMiliseconds = dateTime.ToUnixTimeMilliseconds();

            var trades = await _binanceApiWrapper.RestClient.CallEndpointAsync(BinanceApiWrapper.BinanceApiEndpointNames.AggregatedTrades, symbol, startAndEnd.Start, startAndEnd.End);
            var amount = GetNearestPrice(searchInMiliseconds, trades, items => items.Value<long>("T"), items => items.Value<double>("p"), false);
            if (amount != null)
                return amount.Value;

            //TODO: is this needed because no aggregated trades exists allready
            trades = await _binanceApiWrapper.RestClient.CallEndpointAsync(BinanceApiWrapper.BinanceApiEndpointNames.Trades, symbol);
            amount = GetNearestPrice(searchInMiliseconds, trades, items => items.Value<long>("time"), items => items.Value<double>("price"), true);
            if (amount != null)
                return amount.Value;

            throw new Exception($"No exchange rate for `{symbol}` on `{dateTime}` found.");
        }
        private double? GetNearestPrice(long dateTimeInMiliseconds, string jsonArray, Func<JObject, long> dateTimeSelector, Func<JObject, double> priceSelector, bool searchAll)
        {
            var maxTradeOffset = 3600000;
            var lastTradeTime = 0L;
            var lastTradePrice = 0.0;
            var bestTradePrice = 0.0;
            var bestTradeDifference = long.MaxValue;
            foreach (var trade in JArray.Parse(jsonArray))
            {
                var items = trade as JObject;
                if (items == null)
                    continue;

                var tradeTime = dateTimeSelector(items);
                var tradePrice = priceSelector(items);

                if (searchAll)
                {
                    var tradeDifference = tradeTime - dateTimeInMiliseconds;
                    if (tradeDifference < bestTradeDifference)
                    {
                        bestTradeDifference = tradeDifference;
                        bestTradePrice = tradePrice;
                    }
                }
                else if (tradeTime > dateTimeInMiliseconds)
                {
                    var tradeDifference = tradeTime - dateTimeInMiliseconds;
                    var lastTradeDifference = (lastTradeTime - dateTimeInMiliseconds) * -1;
                    if (tradeDifference > lastTradeDifference)
                    {
                        if (lastTradeDifference >= maxTradeOffset)
                            throw new Exception($"The found exchange rate for is `{lastTradeDifference}` miliseconds older then the searched one.");
                        return lastTradePrice;
                    }

                    if (tradeDifference >= maxTradeOffset)
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

            if (searchAll)
            {
                if (bestTradeDifference >= maxTradeOffset)
                    throw new Exception($"The found exchange rate for is `{bestTradeDifference}` miliseconds older then the searched one.");
                return bestTradePrice;
            }

            if (lastTradeTime == 0)
                return null;

            var lastTradeOffset = (lastTradeTime - dateTimeInMiliseconds) * -1;
            if (lastTradeOffset >= maxTradeOffset)
                throw new Exception($"The found exchange rate for is `{lastTradeOffset}` miliseconds older then the searched one.");

            return null;
        }
        private async Task<bool> DoNotRateLimitGetExchangeRateAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
        {
            var startAndEnd = GetStartAndEndForGetExchangeRate(dateTime);
            var symbol = await _getSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency);
            return _binanceApiWrapper.RestClient.IsCached(BinanceApiWrapper.BinanceApiEndpointNames.AggregatedTrades, symbol, startAndEnd.Start, startAndEnd.End);
        }
        private (DateTime Start, DateTime End) GetStartAndEndForGetExchangeRate(DateTime dateTime)
            => (dateTime.AddMinutes(-5), dateTime.AddMinutes(5));

        public readonly AsyncCachedFunction<List<string>> GetExchangeSymbols;
        private async Task<List<string>> GetExchangeSymbolsAsync()
        {
            var response = await _binanceApiWrapper.RestClient.CallEndpointAsync(BinanceApiWrapper.BinanceApiEndpointNames.ExchangeInfo);
            var json = JObject.Parse(response);
            return json.Value<JArray>("symbols").Select(x =>
            {
                var obj = x as JObject;
                return obj?.Value<string>("symbol");
            }).ToList();
        }

        public async Task<bool> DoExchangeRatesExist(CurrencyModel originCurrency, CurrencyModel targetCurrency)
            => !string.IsNullOrEmpty(await _getSupportedSymbol.ExecuteAsync(originCurrency, targetCurrency));
        #endregion

        #region Private
        private readonly AsyncCachedFunction<CurrencyModel, CurrencyModel, string> _getSupportedSymbol;
        private async Task<string> GetSupportedSymbolAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
        {
            var exchangeRateSymbols = await GetExchangeSymbols.ExecuteAsync();
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
        
        public void Dispose()
        {
            _binanceApiWrapper?.Dispose();
        }

        public static BinanceApi Instance { get; } = new BinanceApi();
    }
}
