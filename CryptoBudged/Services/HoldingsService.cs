using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi;
using NtFreX.RestClient.NET.Flow;

namespace CryptoBudged.Services
{
    public class HoldingsService
    {
        private HoldingsService() { }

        public IList<HoldingModel> CalculateHoldings() => _cachedCalculateHoldings.Execute();

        private readonly ConcurrentFunction<IList<HoldingModel>> _cachedCalculateHoldings = 
            new ConcurrentFunction<IList<HoldingModel>>(
                new CachedFunction<IList<HoldingModel>>(CalculateHoldingsInner, TimeSpan.FromSeconds(5)), 
                1);

        private static IList<HoldingModel> CalculateHoldingsInner()
        {
            var holdings = new List<HoldingModel>();

            foreach (var exchange in ExchangesService.Instance.GetAll())
            {
                if (holdings.All(x => x.Currency.ShortName != exchange.OriginCurrency.ShortName))
                {
                    holdings.Add(new HoldingModel
                    {
                        Amount = 0,
                        Currency = exchange.OriginCurrency
                    });
                }
                if (holdings.All(x => x.Currency.ShortName != exchange.TargetCurrency.ShortName))
                {
                    holdings.Add(new HoldingModel
                    {
                        Amount = 0,
                        Currency = exchange.TargetCurrency
                    });
                }

                holdings.Find(x => x.Currency.ShortName == exchange.OriginCurrency.ShortName).Amount -= exchange.OriginAmount;
                holdings.Find(x => x.Currency.ShortName == exchange.TargetCurrency.ShortName).Amount += exchange.TargetAmount;
            }

            foreach (var depositWithdrawl in DepositWithdrawService.Instance.GetAll())
            {
                if (holdings.All(x => x.Currency.ShortName != depositWithdrawl.Currency.ShortName))
                {
                    holdings.Add(new HoldingModel
                    {
                        Amount = 0,
                        Currency = depositWithdrawl.Currency
                    });
                }

                if (depositWithdrawl.IsTargetAdressMine)
                {
                    holdings.Find(x => x.Currency.ShortName == depositWithdrawl.Currency.ShortName).Amount += depositWithdrawl.Amount - depositWithdrawl.Fees;
                }
                if (depositWithdrawl.IsOriginAdressMine)
                {
                    holdings.Find(x => x.Currency.ShortName == depositWithdrawl.Currency.ShortName).Amount -= depositWithdrawl.Amount;
                }
            }

            holdings.RemoveAll(x => x.Amount == 0);

            Task.WaitAll(holdings.Select(holding => Task.Run(async () =>
            {
                var prices = await CryptocompareApi.Instance.GetCurrentPrices.ExecuteAsync(holding.Currency);
                holding.AmountInBtc = holding.Amount * prices.BTC;
                holding.AmountInChf = holding.Amount * prices.CHF;
                holding.AmountInEth = holding.Amount * prices.ETH;

                holding.PriceInBtc = prices.BTC;
                holding.PriceInChf = prices.CHF;
                holding.PriceInEth = prices.ETH;
            })).ToArray());

            var chfCurrency = CurrencyFactory.Instance.GetByShortName("CHF");
            Task.WaitAll(holdings.Select(holding => Task.Run(async () =>
            {
                var investmentInCurency = await CryptoCurrencyService.Instance.CalculateInvestmentAsync(holding.Currency, chfCurrency);
                var holdingInCurrency = holding.AmountInChf;
                var profitForCurrency = holdingInCurrency - investmentInCurency;
                var profitInPercent = holdingInCurrency / (investmentInCurency / 100.0) - 100.0;

                holding.ProfitInChf = profitForCurrency;
                holding.ProfitInPercent = profitInPercent;
                holding.InvestmentInChf = investmentInCurency;
            })).ToArray());


            return holdings;
        }

        public static HoldingsService Instance { get; } = new HoldingsService();
    }
}
