using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class PieChartHoldingsInChfWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private SeriesCollection _holdingsPieChartSeries;

        public SeriesCollection HoldingsPieChartSeries
        {
            get => _holdingsPieChartSeries;
            set => SetProperty(ref _holdingsPieChartSeries, value);
        }

        public PieChartHoldingsInChfWidgetViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            Task.Run(ReloadPieCharWorker);
        }

        private async Task ReloadPieCharWorker()
        {
            while (true)
            {
                try
                {
                    CalculatePieChart();
                    await Task.Delay(10000);
                }
                catch
                {
                    /* IGNORE */
                }
            }
        }

        private void CalculatePieChart()
        {
            if (HoldingsPieChartSeries == null)
                HoldingsPieChartSeries = new SeriesCollection();

            _dispatcher.Invoke(() =>
            {
                var series = HoldingsPieChartSeries;
                foreach (var holding in HoldingsService.Instance.CalculateHoldings())
                {
                    if (series.Any(x => x.Title == holding.Currency.ToString()))
                    {
                        var value = series.First(x => x.Title == holding.Currency.ToString());
                        value.Values = new ChartValues<double>(new[] {holding.AmountInChf});
                    }
                    else
                    {
                        series.Add(new PieSeries
                        {
                            Values = new ChartValues<double>(new[] {holding.AmountInChf}),
                            Title = holding.Currency.ToString()
                        });
                    }
                }
            });
        }
    }
}
