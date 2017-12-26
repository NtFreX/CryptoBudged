using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Models;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class StackedIncomeInChfWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private SeriesCollection _seriesCollection;
        private string[] _labels;
        private Func<double, string> _formatter;

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set => SetProperty(ref _seriesCollection, value);
        }
        public string[] Labels
        {
            get => _labels;
            set => SetProperty(ref _labels, value);
        }
        public Func<double, string> Formatter
        {
            get => _formatter;
            set => SetProperty(ref _formatter, value);
        }

        public StackedIncomeInChfWidgetViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            Task.Run(ReloadWidgetWorker);
        }

        private async Task ReloadWidgetWorker()
        {
            while (true)
            {
                try
                {
                    CalculateWidged();

                    await Task.Delay(10000);
                }
                catch
                {
                    /* IGNORE */
                }
            }
        }

        private void CalculateWidged()
        {
            //var holdings = HoldingsService.Instance.CalculateHoldings();
            /*var exchanges = ExchangesService.Instance.GetAll();
            var depositWithdrawls = DepositWithdrawService.Instance.GetAll();
            var allCurrencies = exchanges.Select(x => x.OriginCurrency)
                .Concat(exchanges.Select(x => x.TargetCurrency))
                .Concat(depositWithdrawls.Select(x => x.Currency))
                .Distinct(new CurrencyModelEqualityComparer())
                .Where(x => x.IsCryptoCurrency);

            var movements = depositWithdrawls
                .Select(x => ( DateTime: x.DateTime, Value : (object) x ))
                .Concat(exchanges.Select(x => (DateTime: x.DateTime, Value: (object) x)))
                .OrderBy(x => x.DateTime);

            double GetCurrentAmount(List<(DateTime DateTime, HoldingModel Value)> points, CurrencyModel currency)
            {
                var value = points.LastOrDefault(x => x.Value.Currency.ShortName == currency.ShortName);
                if (value.Value == null || value.Value?.Amount == 0)
                    return 0;
                return value.Value.Amount;
            }

            var holdingPoints = new List<( DateTime DateTime, HoldingModel Value )>();
            foreach (var movement in movements)
            {
                if (movement.Value is DepositWithdrawlModel depositWithdrawl)
                {
                    holdingPoints.Add((depositWithdrawl.DateTime, new HoldingModel
                    {
                        Currency = depositWithdrawl.Currency,
                        Amount = GetCurrentAmount(holdingPoints, depositWithdrawl.Currency) + depositWithdrawl.Amount - depositWithdrawl.Fees
                    }));
                }
                else if (movement.Value is ExchangeModel exchange)
                {
                    holdingPoints.Add((exchange.DateTime, new HoldingModel
                    {
                        Currency = exchange.OriginCurrency,
                        Amount = GetCurrentAmount(holdingPoints, exchange.OriginCurrency) - exchange.OriginAmount
                    }));
                    holdingPoints.Add((exchange.DateTime, new HoldingModel
                    {
                        Currency = exchange.TargetCurrency,
                        Amount = GetCurrentAmount(holdingPoints, exchange.TargetCurrency) + exchange.TargetAmount
                    }));
                }
            }

            foreach (var currency in allCurrencies)
            {
                var stackedColumnSeries = new StackedColumnSeries
                {
                    DataLabels = true
                };

                var values = new ChartValues<double>();
                var points = holdingPoints.Where(x => x.Value.Currency.ShortName == currency.ShortName).ToList();
                for(int i = 0; i < points.Count; i++)
                {
                    if(i == 0)
                        continue;

                    var difference = points[i - 1].Value.AmountInChf - points[i].Value.AmountInChf;
                    if (difference < 0)
                    {
                        // a sell happened
                        values.Add(points[i - 1].Value.PriceInChf );
                    }
                    else if (difference > 0)
                    {
                        // a buy happened

                    }

                }
            }*/

            _dispatcher.Invoke(() =>
            {
                SeriesCollection = new SeriesCollection
                {
                    new StackedColumnSeries
                    {
                        Values = new ChartValues<double> {4, 5, 6, 8},
                        StackMode = StackMode.Values, // this is not necessary, values is the default stack mode
                        DataLabels = true
                    },
                    new StackedColumnSeries
                    {
                        Values = new ChartValues<double> {2, 5, 6, 7},
                        StackMode = StackMode.Values,
                        DataLabels = true
                    }
                };

                //adding series updates and animates the chart
                SeriesCollection.Add(new StackedColumnSeries
                {
                    Values = new ChartValues<double> { 6, 2, 7 },
                    StackMode = StackMode.Values
                });

                //adding values also updates and animates
                SeriesCollection[2].Values.Add(4d);

                Labels = new[] { "Chrome", "Mozilla", "Opera", "IE" };
                Formatter = value => value + " Chf";
            });
        }
    }
}
