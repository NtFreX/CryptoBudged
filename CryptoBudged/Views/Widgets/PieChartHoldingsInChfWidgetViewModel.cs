using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class PieChartHoldingsInChfWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private SeriesCollection _holdingsPieChartSeries;
        private bool _isLoading;

        public SeriesCollection HoldingsPieChartSeries
        {
            get => _holdingsPieChartSeries;
            set => SetProperty(ref _holdingsPieChartSeries, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public DelegateCommand RefreshChartCommand { get; }

        public PieChartHoldingsInChfWidgetViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            RefreshChartCommand = new DelegateCommand(ExecuteRefreshChartCommand, () => !IsLoading);

            Task.Run(CalculatePieChartAsync);
        }

        private void ExecuteRefreshChartCommand()
        {
            Task.Run(CalculatePieChartAsync);
        }
        
        private Task CalculatePieChartAsync()
        {
            try
            {
                _dispatcher.Invoke(() =>
                {
                    HoldingsPieChartSeries = new SeriesCollection();
                    IsLoading = true;
                });

                var holdings = HoldingsService.Instance.CalculateHoldings();

                _dispatcher.Invoke(() =>
                {
                    foreach (var holding in holdings)
                    {
                        if (HoldingsPieChartSeries.Any(x => x.Title == holding.Currency.ToString()))
                        {
                            var value = HoldingsPieChartSeries.First(x => x.Title == holding.Currency.ToString());
                            value.Values = new ChartValues<double>(new[] {holding.AmountInChf});
                        }
                        else
                        {
                            HoldingsPieChartSeries.Add(new PieSeries
                            {
                                Values = new ChartValues<double>(new[] {holding.AmountInChf}),
                                Title = holding.Currency.ToString()
                            });
                        }
                    }
                });
            }
            catch (Exception exce)
            {
                Logger.Instance.Log(new Exception("Error while reloading the `PieChartHoldingsInChfWidget`", exce));
            }
            finally
            {
                _dispatcher.Invoke(() => IsLoading = false);
            }

            return Task.CompletedTask;
        }
    }
}
