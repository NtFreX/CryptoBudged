using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.Services;
using Prism.Mvvm;

namespace CryptoBudged.Views.Pages
{
    public class HoldingsViewModel : BindableBase
    {
        private ObservableCollection<HoldingModel> _holdings;

        public ObservableCollection<HoldingModel> Holdings
        {
            get => _holdings;
            set => SetProperty(ref _holdings, value);
        }

        public HoldingsViewModel()
        {
            Task.Run(ReloadHoldingsWorker);
        }

        private async Task ReloadHoldingsWorker()
        {
            while (true)
            {
                try
                {
                    Holdings = new ObservableCollection<HoldingModel>(HoldingsService.Instance.CalculateHoldings());
                    await Task.Delay(5000);
                }
                catch
                {
                    /* IGNORE */
                }
            }
        }
    }
}
