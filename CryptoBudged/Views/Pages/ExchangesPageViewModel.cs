using System;
using System.Collections.ObjectModel;
using System.Linq;
using CryptoBudged.Helpers;
using CryptoBudged.Models;
using CryptoBudged.Services;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Pages
{
    public class ExchangesPageViewModel : BindableBase
    {
        private ObservableCollection<ExchangeModel> _exchanges;

        public DelegateCommand AddExchangeCommand { get; }
        public DelegateCommand<object> DeteleExchangeCommand { get; }
        public DelegateCommand<object> EditExchangeCommand { get; set; }

        public ObservableCollection<ExchangeModel> Exchanges
        {
            get => _exchanges;
            set => SetProperty(ref _exchanges, value);
        }

        public ExchangesPageViewModel()
        {
            Exchanges = new ObservableCollection<ExchangeModel>(ExchangesService.Instance.GetAll());

            AddExchangeCommand = new DelegateCommand(ExecuteAddExchangeCommandAsync);
            DeteleExchangeCommand = new DelegateCommand<object>(ExecuteDeleteExchangeCommand);
            EditExchangeCommand = new DelegateCommand<object>(ExecuteEditExchangeCommandAsync);
        }

        private async void ExecuteAddExchangeCommandAsync()
        {
            var dialog = new ExchangeDialog();
            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success &&
                dialog.DataContext is ExchangeDialogViewModel viewModel)
            {
                Exchanges.Add(new ExchangeModel
                {
                    Id = Guid.NewGuid(),
                    Fees = double.Parse(viewModel.Fees),
                    ExchangeRate = double.Parse(viewModel.ExchangeRate),
                    OriginAmount = double.Parse(viewModel.OriginAmount),
                    TargetAmount = double.Parse(viewModel.TargetAmount),
                    ExchangePlatform = viewModel.SelectedExchangePlatform,
                    OriginCurrency = viewModel.SelectedOriginCurrency,
                    TargetCurrency = viewModel.SelectedTargetCurrency,
                    DateTime = DateTimeHelper.DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime),
                    Note = viewModel.Note
                });

                ExchangesService.Instance.Update(Exchanges);
            }
        }

        private void ExecuteDeleteExchangeCommand(object exchangeModel)
        {
            Exchanges.Remove((ExchangeModel) exchangeModel);
            ExchangesService.Instance.Update(Exchanges);
        }

        private async void ExecuteEditExchangeCommandAsync(object o)
        {
            var dialog = new ExchangeDialog();

            if (!(dialog.DataContext is ExchangeDialogViewModel viewModel))
                return;
            if (!(o is ExchangeModel model))
                return;

            viewModel.Fees = model.Fees.ToString();
            viewModel.ExchangeRate = model.ExchangeRate.ToString();
            viewModel.OriginAmount = model.OriginAmount.ToString();
            viewModel.SelectedDate = model.DateTime;
            viewModel.SelectedExchangePlatform = viewModel.ExchangePlatforms.First(x => x.DisplayName == model.ExchangePlatform.DisplayName);
            viewModel.SelectedOriginCurrency = viewModel.OriginCurrencies.First(x => x.ShortName == model.OriginCurrency.ShortName);
            viewModel.SelectedTargetCurrency = viewModel.TargetCurrencies.First(x => x.ShortName == model.TargetCurrency.ShortName);
            viewModel.SelectedTime = model.DateTime;
            viewModel.TargetAmount = model.TargetAmount.ToString();
            viewModel.Note = model.Note;

            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success)
            {
                var exchange = Exchanges.First(x => x.Id == model.Id);
                exchange.Fees = double.Parse(viewModel.Fees);
                exchange.DateTime = DateTimeHelper.DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime);
                exchange.ExchangePlatform = viewModel.SelectedExchangePlatform;
                exchange.ExchangeRate = double.Parse(viewModel.ExchangeRate);
                exchange.OriginAmount = double.Parse(viewModel.OriginAmount);
                exchange.OriginCurrency = viewModel.SelectedOriginCurrency;
                exchange.TargetAmount = double.Parse(viewModel.TargetAmount);
                exchange.TargetCurrency = viewModel.SelectedTargetCurrency;
                exchange.Note = viewModel.Note;
            }

            Exchanges = new ObservableCollection<ExchangeModel>(Exchanges.ToArray());
            ExchangesService.Instance.Update(Exchanges);
        }
    }
}
