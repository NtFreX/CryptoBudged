using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CryptoBudged.Models;
using CryptoBudged.Services;
using CryptoBudged.Views.Widgets;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Pages
{
    public class HoldingsViewModel : BindableBase
    {
        private ObservableCollection<HoldingModel> _holdings;
        private DelegateCommand<object> _itemClickedCommand;

        public ObservableCollection<HoldingModel> Holdings
        {
            get => _holdings;
            set => SetProperty(ref _holdings, value);
        }

        public DelegateCommand<object> ItemClickedCommand
        {
            get => _itemClickedCommand;
            set => SetProperty(ref _itemClickedCommand, value);
        }

        public HoldingsViewModel()
        {
            ItemClickedCommand = new DelegateCommand<object>(ExecuteItemClickedCommandAsync);

            Task.Run(ReloadHoldingsWorker);
        }

        private async void ExecuteItemClickedCommandAsync(object o)
        {
            if (!(o is ListViewItem item))
                return;
            if (!(item.Content is HoldingModel model))
                return;
            if (!model.Currency.IsCryptoCurrency)
                return;

            var view = new TradingViewWidget
            {
                CurrencyShortName = model.Currency.ShortName,
                Width = 1000,
                Height = 1000
            };

            var dialog = DialogHost.Show(view);

            view.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Escape)
                {
                    DialogHost.CloseDialogCommand.Execute(false, view);
                }
            };

            await dialog;
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
