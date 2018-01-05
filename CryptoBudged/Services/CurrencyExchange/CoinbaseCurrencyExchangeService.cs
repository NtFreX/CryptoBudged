using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi;

namespace CryptoBudged.Services.CurrencyExchange
{
    public class CoinbaseCurrencyExchangeService : ICurrencyExchangeService
    {
        public bool IsInitialized { get; } = true;
        public Task InitializeAsync(Action<double> progressCallback) => Task.CompletedTask;

        public async Task<bool> CanConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
        {
            var currencies = await CoinbaseApi.Instance.GetCurrencies.ExecuteAsync();
            return currencies.Any(x => x == originCurrency.ShortName) &&
                   currencies.Any(x => x == targetCurrency.ShortName) &&
                   ((originCurrency.IsCryptoCurrency && !targetCurrency.IsCryptoCurrency) ||
                    (!originCurrency.IsCryptoCurrency && targetCurrency.IsCryptoCurrency));
        }
        public async Task<TimeSpan> IsRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
            => await CoinbaseApi.Instance.IsGetSpotPriceRateLimitedAsync(originCurrency, targetCurrency, dateTime);
        public async Task<double> ConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime)
        {
            if (!await CanConvertAsync(originCurrency, targetCurrency))
                throw new ArgumentException();

            var price = await CoinbaseApi.Instance.GetSpotPriceAsync(originCurrency, targetCurrency, dateTime);
            if (originCurrency.IsCryptoCurrency)
            {
                return amount * price;
            }
            //TODO: look if this works and how to solve better
            return 1.0 / price * amount;
        }
    }
}
