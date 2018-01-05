
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Factories;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class InvestmentInTotalWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private double _investment;

        public double Investment
        {
            get => _investment;
            set => SetProperty(ref _investment, value);
        }

        public InvestmentInTotalWidgetViewModel()
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
                    var investment = 0.0;

                    foreach (var currency in allUsedCurrencies)
                    {
                        var investmentInCurency = await CryptoCurrencyService.Instance.CalculateInvestmentAsync(currency, targetCurrency);
                        investment += investmentInCurency;
                    }

                    _dispatcher.Invoke(() =>
                    {
                        Investment = investment;
                    });


                    await Task.Delay(5000);
                }
                catch (Exception exce)
                {
                    Logger.Instance.Log(new Exception("Error while reloading the `InvestmentInTotalWidget`", exce));
                }
            }
        }
    }
}
