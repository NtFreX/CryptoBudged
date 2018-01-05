using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class HoldingsInChfWidgetViewModel : BindableBase
    {
        private double _totalAmountInCHF;

        public double TotalAmountInCHF
        {
            get => _totalAmountInCHF;
            set => SetProperty(ref _totalAmountInCHF, value);
        }

        public HoldingsInChfWidgetViewModel()
        {
            Task.Run(ReloadWidgetWorker);
        }

        private async Task ReloadWidgetWorker()
        {
            while (true)
            {
                try
                {
                    TotalAmountInCHF = HoldingsService.Instance.CalculateHoldings().Sum(x => x.AmountInChf);
                    await Task.Delay(5000);
                }
                catch (Exception exce)
                {
                    Logger.Instance.Log(new Exception("Error while reloading the `HoldingsInChfWidget`", exce));
                }
            }
        }
    }
}
