using System;
using System.Threading.Tasks;
using CryptoBudged.Models;

namespace CryptoBudged.Services.CurrencyExchange
{
    public interface ICurrencyExchangeService
    {
        bool IsInitialized { get; }
        Task InitializeAsync(Action<double> progressCallback);

        Task<bool> CanConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency);
        Task<TimeSpan> IsRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime);
        Task<double> ConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime);
    }
}