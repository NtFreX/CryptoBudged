using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class StackedCryptoDevelopmentWidgetViewModel : BindableBase
    {
        private SeriesCollection _historicalHoldingsLineChartSeries;

        public SeriesCollection HistoricalHoldingsLineChartSeries
        {
            get => _historicalHoldingsLineChartSeries;
            set => SetProperty(ref _historicalHoldingsLineChartSeries, value);
        }

        public StackedCryptoDevelopmentWidgetViewModel()
        {
            Task.Run(ReloadWidgetWorker);
        }

        private async Task ReloadWidgetWorker()
        {
            while (true)
            {
                try
                {
                    CalculateLineChart();
                    await Task.Delay(10000);
                }
                catch (Exception exce)
                {
                    Console.WriteLine($"Error while reloading the `StackedCryptoDevelopmentWidget`: {exce.Message}");
                }
            }
        }

        private void CalculateLineChart()
        {
            var depositsWithdrawls = DepositWithdrawService.Instance.GetAll().OrderBy(x => x.DateTime);
            var exchanges = ExchangesService.Instance.GetAll().OrderBy(x => x.DateTime);

            var firstDepositsWithdrawl = depositsWithdrawls.First().DateTime;
            var firstExchange = exchanges.First().DateTime;
            var firstMovement = firstExchange > firstDepositsWithdrawl ? firstDepositsWithdrawl : firstExchange;
            var oneDayBeforeFirstMovement = firstMovement.Date - new TimeSpan(1, 0, 0);

            var holdingPoints = new List<KeyValuePair<DateTime, HoldingModel>>();
            var movements = new List<KeyValuePair<DateTime, List<object>>>();
            foreach (var depositsWithdrawl in depositsWithdrawls)
            {
                if (holdingPoints.All(x => x.Value.Currency.ShortName != depositsWithdrawl.Currency.ShortName))
                {
                    holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(oneDayBeforeFirstMovement,
                        new HoldingModel
                        {
                            Amount = 0,
                            Currency = depositsWithdrawl.Currency
                        }));
                }
                if (movements.All(x => x.Key != depositsWithdrawl.DateTime))
                {
                    movements.Add(new KeyValuePair<DateTime, List<object>>(depositsWithdrawl.DateTime, new List<object>(new object[] { depositsWithdrawl })));
                }
                else
                {
                    movements.First(x => x.Key == depositsWithdrawl.DateTime).Value.Add(depositsWithdrawl);
                }
            }
            foreach (var exchange in exchanges)
            {
                if (holdingPoints.All(x => x.Value.Currency.ShortName != exchange.OriginCurrency.ShortName))
                {
                    holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(oneDayBeforeFirstMovement,
                        new HoldingModel
                        {
                            Amount = 0,
                            Currency = exchange.OriginCurrency
                        }));
                }
                if (holdingPoints.All(x => x.Value.Currency.ShortName != exchange.TargetCurrency.ShortName))
                {
                    holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(oneDayBeforeFirstMovement,
                        new HoldingModel
                        {
                            Amount = 0,
                            Currency = exchange.TargetCurrency
                        }));
                }
                if (movements.All(x => x.Key != exchange.DateTime))
                {
                    movements.Add(new KeyValuePair<DateTime, List<object>>(exchange.DateTime, new List<object>(new object[] { exchange })));
                }
                else
                {
                    movements.First(x => x.Key == exchange.DateTime).Value.Add(exchange);
                }
            }
            var orderedMovements = movements.OrderBy(x => x.Key);
            var allCurrencies = holdingPoints.Select(x => x.Value.Currency).ToArray();
            //holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(orderedMovements.First().Key - new TimeSpan(0, 1, 0), new HoldingModel()));

            double GetCurrentAmount(List<KeyValuePair<DateTime, HoldingModel>> points, CurrencyModel currency)
            {
                var value = points.LastOrDefault(x => x.Value.Currency.ShortName == currency.ShortName);
                if (value.Value == null || value.Value?.Amount == 0)
                    return 0;
                return value.Value.Amount;
            }

            foreach (var movement in orderedMovements)
            {
                foreach (var movementValue in movement.Value)
                {
                    if (movementValue is DepositWithdrawlModel depositWithdrawl)
                    {
                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(depositWithdrawl.DateTime, new HoldingModel
                        {
                            Currency = depositWithdrawl.Currency,
                            Amount = GetCurrentAmount(holdingPoints, depositWithdrawl.Currency) + depositWithdrawl.Amount - depositWithdrawl.Fees
                        }));
                    }
                    else if (movementValue is ExchangeModel exchange)
                    {
                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(exchange.DateTime, new HoldingModel
                        {
                            Currency = exchange.OriginCurrency,
                            Amount = GetCurrentAmount(holdingPoints, exchange.OriginCurrency) - exchange.OriginAmount
                        }));
                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(exchange.DateTime, new HoldingModel
                        {
                            Currency = exchange.TargetCurrency,
                            Amount = GetCurrentAmount(holdingPoints, exchange.TargetCurrency) + exchange.TargetAmount
                        }));
                    }
                }
            }

            // smoth out lines by adding a point for each currency and day
            var currentDate = oneDayBeforeFirstMovement;
            while (currentDate.Date < DateTime.Now.Date)
            {
                foreach (var currency in allCurrencies)
                {
                    if (holdingPoints.All(x => x.Key.Date != currentDate || x.Value.Currency.ShortName != currency.ShortName))
                    {
                        var lastHoldingPoint = holdingPoints
                            .Where(x => x.Value.Currency.ShortName == currency.ShortName)
                            .Where(x => x.Key < currentDate.Date)
                            .OrderByDescending(x => x.Key)
                            .FirstOrDefault();
                        var lastAmount = lastHoldingPoint.Value?.Amount ?? 0;

                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(currentDate.Date, new HoldingModel
                        {
                            Currency = currency,
                            Amount = lastAmount
                        }));
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            // delete middle value if three values in a row are the same
            foreach (var currency in allCurrencies)
            {
                var currentHoldingPoints = holdingPoints
                    .Where(x => x.Value.Currency.ShortName == currency.ShortName)
                    .OrderBy(x => x.Key)
                    .ToList();
                var lastHoldingPoint = default(KeyValuePair<DateTime, HoldingModel>);
                for (var i = 0; i < currentHoldingPoints.Count - 1; i++)
                {
                    if (lastHoldingPoint.Value != null)
                    {
                        if (lastHoldingPoint.Value.Amount == currentHoldingPoints[i].Value.Amount &&
                            currentHoldingPoints[i].Value.Amount == currentHoldingPoints[i + 1].Value.Amount)
                        {
                            currentHoldingPoints.RemoveAt(i);
                            i--;
                        }
                    }
                    lastHoldingPoint = currentHoldingPoints[i];
                }

                holdingPoints.RemoveAll(x => x.Value.Currency.ShortName == currency.ShortName);
                holdingPoints.AddRange(currentHoldingPoints);
            }

            var seriesCollection = new SeriesCollection();
            foreach (var currency in allCurrencies)
            {
                seriesCollection.Add(new StackedAreaSeries
                {
                    Title = currency.ToString(),
                    Values = new ChartValues<DateTimePoint>(holdingPoints
                        .Where(x => x.Value.Currency.ShortName == currency.ShortName)
                        .OrderBy(x => x.Key)
                        .Select(x => new DateTimePoint(x.Key, x.Value.Amount)))
                });
            }


            HistoricalHoldingsLineChartSeries = seriesCollection;
        }
    }
}
