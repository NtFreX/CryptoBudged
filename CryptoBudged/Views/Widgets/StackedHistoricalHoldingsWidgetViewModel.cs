using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Extensions;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using CryptoBudged.Services;
using CryptoBudged.Services.CurrencyExchange;
using CryptoBudged.ThirdPartyApi;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class StackedHistoricalHoldingsWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private SeriesCollection _chartSeries;
        private bool _isLoading;

        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set => SetProperty(ref _chartSeries, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public DelegateCommand RefreshChartCommand { get; }

        public StackedHistoricalHoldingsWidgetViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            RefreshChartCommand = new DelegateCommand(ExecuteRefreshChartCommand, () => !IsLoading);

            XFormatter = val => new DateTime((long) val).ToString("dd.MM.yyyy hh:mm");
            YFormatter = val => val.ToString();

            Task.Run(CalculateChartAsync);
        }

        private void ExecuteRefreshChartCommand()
        {
            Task.Run(CalculateChartAsync);
        }

        private async Task CalculateChartAsync()
        {
            try
            {
                _dispatcher.Invoke(() =>
                {
                    ChartSeries = new SeriesCollection();
                    IsLoading = true;
                });

                var depositWithdrawl = DepositWithdrawService.Instance.GetAll().OrderBy(x => x.DateTime);
                var exchanges = ExchangesService.Instance.GetAll().OrderBy(x => x.DateTime);

                var firstDepositWithdrawl = depositWithdrawl.FirstOrDefault()?.DateTime;
                var firstExchange = exchanges.FirstOrDefault()?.DateTime;
                if (firstExchange == null && firstDepositWithdrawl == null)
                    return;

                var allUsedCurrencies = CryptoCurrencyService.Instance.GetAllUsedCurrencies().ToList();
                var targetCurrency = CurrencyFactory.Instance.GetByShortName("CHF");
                var firstAction = firstExchange != null && firstDepositWithdrawl != null
                    ? (firstExchange > firstDepositWithdrawl ? firstDepositWithdrawl : firstExchange)
                    : (firstExchange ?? firstDepositWithdrawl);
                var currentDate = firstAction.Value;
                var totalHours = (DateTime.Now.ToUnixTimeSeconds() - currentDate.ToUnixTimeSeconds()) / 60.0 / 60.0;
                var stepSizeInHours = Math.Round(totalHours / 100); // use allways 100 datapoints per currency

                foreach (var currency in allUsedCurrencies)
                {
                    _dispatcher.Invoke(() =>
                    {
                        ChartSeries.Add(new StackedAreaSeries
                        {
                            Title = currency.ToString(),
                            Values = new ChartValues<DateTimePoint>(),
                            LineSmoothness = 0
                        });
                    });
                }

                while (currentDate < DateTime.Now)
                {
                    var holdings = GetHoldings(currentDate);
                    var itemsToAdd = new List<(CurrencyModel Currency, DateTime DateTime, double Amount)>();
                    foreach (var currency in allUsedCurrencies)
                    {
                        if (holdings.All(x => x.Key != currency.ShortName))
                        {
                            itemsToAdd.Add((currency, currentDate, 0.0));
                        }
                    }

                    await Task.WhenAll(holdings.Select(holding => Task.Run(async () =>
                    {
                        var currency = CurrencyFactory.Instance.GetByShortName(holding.Key);
                        var amount = await CurrencyExchangeFactory.Instance.TryConvertAsync(currency, targetCurrency, holding.Value, currentDate);

                        itemsToAdd.Add((currency, currentDate, amount));
                    })));

                    AddToChart(itemsToAdd);
                    currentDate = currentDate.AddHours(stepSizeInHours);
                }

                var now = DateTime.Now;
                var currentHoldings = GetHoldings(now);
                var currentHoldingsItems = new List<(CurrencyModel Currency, DateTime DateTime, double Amount)>();
                await Task.WhenAll(allUsedCurrencies.Select(currency => Task.Run(async () =>
                {
                    var prices = await CryptocompareApi.Instance.GetCurrentPrices.ExecuteAsync(currency);
                    var amount = currentHoldings.Any(x => x.Key == currency.ShortName)
                        ? currentHoldings.First(x => x.Key == currency.ShortName).Value
                        : 0.0;
                    currentHoldingsItems.Add((currency, now, prices.CHF * amount));
                })));
                AddToChart(currentHoldingsItems);
            }
            catch (Exception exce)
            {
                Logger.Instance.Log(new Exception("Error while reloading the `StackedHistoricalHoldingsWidget`", exce));
            }
            finally 
            {
                _dispatcher.Invoke(() => IsLoading = false);
            }
        }

        private void AddToChart(List<(CurrencyModel Currency, DateTime DateTime, double Value)> values)
        {
            _dispatcher.Invoke(() =>
            {
                foreach (var value in values)
                {
                    if (!(ChartSeries.First(x => x.Title == value.Currency.ToString()).Values is ChartValues<DateTimePoint> chartValues)) return;
                    chartValues.Add(new DateTimePoint(value.DateTime, value.Value));
                }
            });
        }
        private Dictionary<string, double> GetHoldings(DateTime dateTime)
        {
            var holdings = new Dictionary<string, double>();

            foreach (var exchange in ExchangesService.Instance.GetAll())
            {
                if (exchange.DateTime > dateTime)
                    continue;

                if (!holdings.ContainsKey(exchange.OriginCurrency.ShortName))
                {
                    holdings.Add(exchange.OriginCurrency.ShortName, 0);
                }
                if (!holdings.ContainsKey(exchange.TargetCurrency.ShortName))
                {
                    holdings.Add(exchange.TargetCurrency.ShortName, 0);
                }

                holdings[exchange.OriginCurrency.ShortName] -= exchange.OriginAmount;
                holdings[exchange.TargetCurrency.ShortName] += exchange.TargetAmount;
            }

            foreach (var depositWithdrawl in DepositWithdrawService.Instance.GetAll())
            {
                if (depositWithdrawl.DateTime > dateTime)
                    continue;

                if (!holdings.ContainsKey(depositWithdrawl.Currency.ShortName))
                {
                    holdings.Add(depositWithdrawl.Currency.ShortName, 0);
                }

                if (depositWithdrawl.IsTargetAdressMine)
                {
                    holdings[depositWithdrawl.Currency.ShortName] += depositWithdrawl.Amount - depositWithdrawl.Fees;
                }
                if (depositWithdrawl.IsOriginAdressMine)
                {
                    holdings[depositWithdrawl.Currency.ShortName] -= depositWithdrawl.Amount;
                }
            }

            holdings.Where(x => x.Value == 0).ToList().ForEach(x => holdings.Remove(x.Key));
            return holdings;
        }
    }
}
