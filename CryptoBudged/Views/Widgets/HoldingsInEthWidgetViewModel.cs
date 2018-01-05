using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class HoldingsInEthWidgetViewModel : BindableBase
    {
        private double _totalAmountInETH;

        public double TotalAmountInETH
        {
            get => _totalAmountInETH;
            set => SetProperty(ref _totalAmountInETH, value);
        }
        
        public HoldingsInEthWidgetViewModel()
        {
            Task.Run(ReloadWidgetWorker);
        }

        private async Task ReloadWidgetWorker()
        {
            while (true)
            {
                try
                {
                    TotalAmountInETH = HoldingsService.Instance.CalculateHoldings().Sum(x => x.AmountInEth);
                    await Task.Delay(5000);
                }
                catch (Exception exce)
                {
                    Logger.Instance.Log(new Exception("Error while reloading the `HoldingsInEthWidget`", exce));
                }
            }
        }
    }
}
