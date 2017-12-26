using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Helpers;
using CryptoBudged.Models;

namespace CryptoBudged.Services
{
    public class HoldingsService
    {
        private HoldingsService() { }

        public IList<HoldingModel> CalculateHoldings() => _cachedCalculateHoldings.Execute();

        private readonly CachedFunction<IList<HoldingModel>> _cachedCalculateHoldings = new CachedFunction<IList<HoldingModel>>(
            () =>
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

                    holdings.Find(x => x.Currency.ShortName == exchange.OriginCurrency.ShortName).Amount -=
                        exchange.OriginAmount;
                    holdings.Find(x => x.Currency.ShortName == exchange.TargetCurrency.ShortName).Amount +=
                        exchange.TargetAmount;
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

                    holdings.Find(x => x.Currency.ShortName == depositWithdrawl.Currency.ShortName).Amount +=
                        depositWithdrawl.Amount - depositWithdrawl.Fees;

                    if (depositWithdrawl.WithDrawFromHoldings)
                    {
                        holdings.Find(x => x.Currency.ShortName == depositWithdrawl.Currency.ShortName).Amount -=
                            depositWithdrawl.Amount;
                    }
                }

                holdings.RemoveAll(x => x.Amount == 0);

                var cryptoCurrencyService = new CryptoCurrencyService();
                var tasks = new List<Task>();
                foreach (var holding in holdings)
                {
                    var tmpHolding = holding;
                    tasks.Add(Task.Run(async () =>
                    {
                        var prices = await cryptoCurrencyService.GetPriceOfCurrencyAsync(holding.Currency);
                        tmpHolding.AmountInBtc = tmpHolding.Amount * prices.BTC;
                        tmpHolding.AmountInChf = tmpHolding.Amount * prices.CHF;
                        tmpHolding.AmountInEth = tmpHolding.Amount * prices.ETH;

                        tmpHolding.PriceInBtc = prices.BTC;
                        tmpHolding.PriceInChf = prices.CHF;
                        tmpHolding.PriceInEth = prices.ETH;
                    }));
                }

                if (!Task.WaitAll(tasks.ToArray(), new TimeSpan(0, 1, 0)))
                {
                    throw new TaskCanceledException();
                }

                return holdings;
            }, TimeSpan.FromSeconds(5));

        public static HoldingsService Instance { get; } = new HoldingsService();
    }
}
