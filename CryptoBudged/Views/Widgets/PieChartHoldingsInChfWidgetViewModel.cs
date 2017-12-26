using System.Linq;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class PieChartHoldingsInChfWidgetViewModel : BindableBase
    {
        private SeriesCollection _holdingsPieChartSeries;
        
        public SeriesCollection HoldingsPieChartSeries
        {
            get => _holdingsPieChartSeries;
            set => SetProperty(ref _holdingsPieChartSeries, value);
        }
        
        public DelegateCommand RefreshChartCommand { get; }

        public PieChartHoldingsInChfWidgetViewModel()
        {
            RefreshChartCommand = new DelegateCommand(ExecuteRefreshChartCommand);

            CalculatePieChart();
        }

        private void ExecuteRefreshChartCommand()
        {
            CalculatePieChart();
        }
        
        private void CalculatePieChart()
        {
            if (HoldingsPieChartSeries == null)
                HoldingsPieChartSeries = new SeriesCollection();
            
            foreach (var holding in HoldingsService.Instance.CalculateHoldings())
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
        }
    }
}
