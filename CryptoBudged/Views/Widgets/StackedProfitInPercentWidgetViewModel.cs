using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class StackedProfitInPercentWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private SeriesCollection _seriesCollection;
        private bool _isLoading;

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set => SetProperty(ref _seriesCollection, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public DelegateCommand RefreshChartCommand { get; }

        public StackedProfitInPercentWidgetViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            RefreshChartCommand = new DelegateCommand(ExecuteRefreshChartCommand, () => !IsLoading);
            
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
                    SeriesCollection = new SeriesCollection();
                    IsLoading = true;
                });

                var targetCurrency = CurrencyFactory.Instance.GetByShortName("CHF");
                var allUsedCurrencies = CryptoCurrencyService.Instance.GetAllUsedCurrencies().ToArray();
                var allHoldings = HoldingsService.Instance.CalculateHoldings().OrderByDescending(x => x.ProfitInPercent)
                    .ToList();

                var profits = new List<(CurrencyModel Currency, double ProfitInPercent)>();
                foreach (var currency in allUsedCurrencies)
                {
                    var profitInPercent = await GetProfitInPercentAsync(currency, targetCurrency, allHoldings);
                    profits.Add((currency, profitInPercent));
                }
                profits = profits.OrderByDescending(x => x.ProfitInPercent).ToList();

                _dispatcher.Invoke(() =>
                {
                    var newCollection = new SeriesCollection();
                    foreach (var profit in profits)
                    {
                        newCollection.Add(new RowSeries
                        {
                            Title = profit.Currency.ToString(),
                            Values = new ChartValues<double> {profit.ProfitInPercent}
                        });
                    }
                    SeriesCollection = newCollection;
                });
            }
            catch (Exception exce)
            {
                Logger.Instance.Log(new Exception("Error while reloading the `StackedProfitInPercentWidget`", exce));
            }
            finally
            {

                _dispatcher.Invoke(() => IsLoading = false);
            }
        }

        private async Task<double> GetProfitInPercentAsync(CurrencyModel currency, CurrencyModel targetCurrency, IList<HoldingModel> allHoldings)
        {
            var investmentInCurrency = await CryptoCurrencyService.Instance.CalculateInvestmentAsync(currency, targetCurrency);
            var holdingsInCurrency = allHoldings.FirstOrDefault(x => x.Currency.ShortName == currency.ShortName)?.AmountInChf ?? 0.0;
            return holdingsInCurrency / (investmentInCurrency / 100.0) - 100;
        }
    }
}
