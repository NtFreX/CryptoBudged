using System;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi;

namespace CryptoBudged.Services.CurrencyExchange
{
    public class CloudFiatCurrencyExchangeService : ICurrencyExchangeService
    {
        public bool IsInitialized { get; } = true;
        public Task InitializeAsync(Action<double> progressCallback) => Task.CompletedTask;

        public Task<bool> CanConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
            => Task.FromResult(!originCurrency.IsCryptoCurrency && !targetCurrency.IsCryptoCurrency);
        public async Task<TimeSpan> IsRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
            => await FixerApi.Instance.IsGetExchangeRateRateLimitedAsync(originCurrency, dateTime);
        public async Task<double> ConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime)
        {
            var exchangeRate = await FixerApi.Instance.GetExchangeRateAsync(originCurrency, targetCurrency, dateTime);
            return exchangeRate * amount;
        }
    }
}