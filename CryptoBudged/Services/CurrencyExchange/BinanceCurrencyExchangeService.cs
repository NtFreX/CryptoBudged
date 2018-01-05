using System;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi;

namespace CryptoBudged.Services.CurrencyExchange
{
    public class BinanceCurrencyExchangeService : ICurrencyExchangeService
    {
        public bool IsInitialized { get; } = true;
        public Task InitializeAsync(Action<double> progressCallback) => Task.CompletedTask;

        public async Task<bool> CanConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
            => await BinanceApi.Instance.DoExchangeRatesExist(originCurrency, targetCurrency);
        public async Task<TimeSpan> IsRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
            => await BinanceApi.Instance.GetExchangeRate.IsRateLimitedAsync(originCurrency, targetCurrency, dateTime);
        public async Task<double> ConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime)
        {
            if (!await CanConvertAsync(originCurrency, targetCurrency))
                throw new ArgumentException();

            var exchangeRate = await BinanceApi.Instance.GetExchangeRate.ExecuteAsync(originCurrency, targetCurrency, dateTime);
            //TODO: look if this works and how to solve better
            return amount * exchangeRate;
        }
    }
}