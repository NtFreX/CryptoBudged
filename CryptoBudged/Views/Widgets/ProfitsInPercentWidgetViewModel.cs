using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Factories;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class ProfitsInPercentWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private double _profitInPercent;

        public double ProfitInPercent
        {
            get => _profitInPercent;
            set => SetProperty(ref _profitInPercent, value);
        }

        public ProfitsInPercentWidgetViewModel()
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
                    var targetCurrency = CurrencyFactory.Instance.GetByShortName("CHF");
                    var allUsedCurrencies = CryptoCurrencyService.Instance.GetAllUsedCurrencies().ToArray();
                    var allHoldings = HoldingsService.Instance.CalculateHoldings();
                    var holdings = allHoldings.Sum(x => x.AmountInChf);
                    var investment = 0.0;

                    foreach (var currency in allUsedCurrencies)
                    {
                        var investmentInCurency = await CryptoCurrencyService.Instance.CalculateInvestmentAsync(currency, targetCurrency);
                        investment += investmentInCurency;
                    }

                    _dispatcher.Invoke(() =>
                    {
                        ProfitInPercent = holdings / (investment / 100.0) - 100;
                    });

                    
                    await Task.Delay(5000);
                }
                catch (Exception exce)
                {
                    Logger.Instance.Log(new Exception("Error while reloading the `ProfitsInPercentWidget`", exce));
                }
            }
        }
    }
}
