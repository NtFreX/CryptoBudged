using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Factories;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class ProfitsInTotalWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;
        
        private double _profit;

        public double Profit
        {
            get => _profit;
            set => SetProperty(ref _profit, value);
        }

        public ProfitsInTotalWidgetViewModel()
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
                    var profit = 0.0;

                    foreach (var currency in allUsedCurrencies)
                    {
                        var investmentInCurency = await CryptoCurrencyService.Instance.CalculateInvestmentAsync(currency, targetCurrency);
                        var holdingInCurrency = allHoldings.FirstOrDefault(x => x.Currency.ShortName == currency.ShortName)?.AmountInChf ?? 0.0;
                        var profitForCurrency = holdingInCurrency - investmentInCurency;

                        profit += profitForCurrency;
                    }

                    _dispatcher.Invoke(() =>
                    {
                        Profit = profit;
                    });

                    await Task.Delay(5000);
                }
                catch (Exception exce)
                {
                    Logger.Instance.Log(new Exception("Error while reloading the `ProfitsInTotalWidget`", exce));
                }
            }
        }
    }
}
