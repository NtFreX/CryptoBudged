using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class HoldingsInBtcWidgetViewModel : BindableBase
    {
        private double _totalAmountInBTC;

        public double TotalAmountInBTC
        {
            get => _totalAmountInBTC;
            set => SetProperty(ref _totalAmountInBTC, value);
        }

        public HoldingsInBtcWidgetViewModel()
        {
            Task.Run(ReloadWidgetWorker);
        }

        private async Task ReloadWidgetWorker()
        {
            while (true)
            {
                try
                {
                    TotalAmountInBTC = HoldingsService.Instance.CalculateHoldings().Sum(x => x.AmountInBtc);
                    await Task.Delay(5000);
                }
                catch (Exception exce)
                {
                    Logger.Instance.Log(new Exception("Error while reloading the `HoldingsInBtcWidget`", exce));
                }
            }
        }
}
}
