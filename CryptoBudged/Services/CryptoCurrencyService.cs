using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Factories;
using CryptoBudged.Helpers;
using CryptoBudged.Models;
using CryptoBudged.Services.CurrencyExchange;

namespace CryptoBudged.Services
{
    public class CryptoCurrencyService
    {
        private readonly AsyncCachedFunction<CurrencyModel, CurrencyModel, double> _calculateInvestmentFunc = new AsyncCachedFunction<CurrencyModel, CurrencyModel, double>(CalculateInvestmentInnerAsync, TimeSpan.FromSeconds(10));
        private static async Task<double> CalculateInvestmentInnerAsync(CurrencyModel currency, CurrencyModel targetCurrency)
        {
            var movements = DepositWithdrawService.Instance.GetAll()
                .Where(x => x.Currency.ShortName == currency.ShortName)
                .Where(x => !x.IsOriginAdressMine && x.IsTargetAdressMine);

            var investment = 0.0;
            foreach (var movment in movements)
            {
                investment += await CurrencyExchangeFactory.Instance.TryConvertAsync(movment.Currency, targetCurrency, movment.Amount, movment.DateTime);
            }

            var buyAmount = await CalculateBuyAmountAsync(currency, targetCurrency);
            var sellAmount = await CalculateSellAmountAsync(currency, targetCurrency);

            return investment - sellAmount.Income + buyAmount.Coast;
        }
        private static async Task<(double Coast, double Amount)> CalculateBuyAmountAsync(CurrencyModel currency, CurrencyModel targetCurrency)
        {
            var usdCurrency = CurrencyFactory.Instance.GetByShortName("USD");
            var exchanges = ExchangesService.Instance.GetAll();
            var coast = 0.0;
            var holdings = 0.0;
            foreach (var exchange in exchanges)
            {
                if (exchange.TargetCurrency.ShortName == currency.ShortName)
                {
                    // buy
                    if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.OriginCurrency, targetCurrency))
                    {
                        coast += await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.OriginCurrency, targetCurrency, exchange.OriginAmount, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.TargetCurrency, targetCurrency))
                    {
                        coast += await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.TargetCurrency, targetCurrency, exchange.TargetAmount + exchange.Fees, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.OriginCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.OriginCurrency, usdCurrency, exchange.OriginAmount, exchange.DateTime);
                        coast += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.TargetCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.TargetCurrency, usdCurrency, exchange.TargetAmount + exchange.Fees, exchange.DateTime);
                        coast += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else
                    {
                        // the profit cannot be calculated because no exchange rates exits
                        throw new ArgumentException($"No exchange rate for {exchange.OriginCurrency} exists");
                    }

                    holdings += exchange.TargetAmount;
                }
            }
            return (Coast: coast, Amount: holdings);
        }
        private static async Task<(double Income, double Amount)> CalculateSellAmountAsync(CurrencyModel currency, CurrencyModel targetCurrency)
        {
            var exchanges = ExchangesService.Instance.GetAll();
            var usdCurrency = CurrencyFactory.Instance.GetByShortName("USD");
            var income = 0.0;
            var amount = 0.0;
            foreach (var exchange in exchanges)
            {
                if (exchange.OriginCurrency.ShortName == currency.ShortName)
                {
                    // sell
                    if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.TargetCurrency, targetCurrency))
                    {
                        income += await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.TargetCurrency, targetCurrency, exchange.TargetAmount, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.OriginCurrency, targetCurrency))
                    {
                        income += await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.OriginCurrency, targetCurrency, exchange.OriginAmount, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.TargetCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.TargetCurrency, usdCurrency, exchange.TargetAmount, exchange.DateTime);
                        income += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.OriginCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.OriginCurrency, usdCurrency, exchange.OriginAmount, exchange.DateTime);
                        income += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else
                    {
                        // the profit cannot be calculated because no exchange rates exits
                        throw new ArgumentException($"No exchange rate for {exchange.OriginCurrency} exists");
                    }

                    amount += exchange.OriginAmount;
                }
            }
            return (Income: income, Amount: amount);
        }

        private CryptoCurrencyService() { }
        
        public IEnumerable<CurrencyModel> GetAllUsedCurrencies()
        {
            var exchanges = ExchangesService.Instance.GetAll();
            var depositWithdraws = DepositWithdrawService.Instance.GetAll();
            var allUsedCurrencies = exchanges.Select(x => x.OriginCurrency).Concat(
                    exchanges.Select(x => x.TargetCurrency)).Concat(
                    depositWithdraws.Select(x => x.Currency))
                .Distinct(new CurrencyModelEqualityComparer());
            return allUsedCurrencies;
        }
        public async Task<double> CalculateInvestmentAsync(CurrencyModel currency, CurrencyModel targetCurrency)
            => await _calculateInvestmentFunc.ExecuteAsync(currency, targetCurrency);
        
        public static CryptoCurrencyService Instance { get; } = new CryptoCurrencyService();
    }
}
