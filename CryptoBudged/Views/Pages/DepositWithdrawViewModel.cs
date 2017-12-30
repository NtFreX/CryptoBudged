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
    public class DepositWithdrawViewModel : BindableBase
    {
        private ObservableCollection<DepositWithdrawlModel> _depositWithdrawls;

        public ObservableCollection<DepositWithdrawlModel> DepositWithdrawls
        {
            get => _depositWithdrawls;
            set => SetProperty(ref _depositWithdrawls, value);
        }

        public DelegateCommand AddDepositWithdrawlCommand { get; }
        public DelegateCommand<object> DeleteDepositWithdrawlCommand { get; set; }
        public DelegateCommand<object> EditDepositWithdrawlCommand { get; set; }

        public DepositWithdrawViewModel()
        {
            DepositWithdrawls = new ObservableCollection<DepositWithdrawlModel>(DepositWithdrawService.Instance.GetAll());


            AddDepositWithdrawlCommand = new DelegateCommand(ExecuteAddDepositWithdrawl);
            DeleteDepositWithdrawlCommand = new DelegateCommand<object>(ExecuteDeleteDepositWithdrawlCommand);
            EditDepositWithdrawlCommand = new DelegateCommand<object>(ExecuteEditDepositWithdrawlCommandAsync);
        }

        private async void ExecuteEditDepositWithdrawlCommandAsync(object o)
        {
            var dialog = new DepositWithdrawlDialog();

            if (!(dialog.DataContext is DepositWithdrawlDialogViewModel viewModel))
                return;
            if (!(o is DepositWithdrawlModel model))
                return;

            viewModel.Fees = model.Fees.ToString();
            viewModel.Amount = model.Amount.ToString();
            viewModel.OriginAdress = model.OriginAdress;
            viewModel.SelectedDate = model.DateTime;
            viewModel.SelectedTargetPlatform = viewModel.TargetPlatform.First(x => x.DisplayName == model.TargetPlatform.DisplayName);
            viewModel.SelectedOriginPlatform = viewModel.OriginPlatform.First(x => x.DisplayName == model.OriginPlatform.DisplayName);
            viewModel.SelectedCurrency = viewModel.Currencies.First(x => x.ShortName == model.Currency.ShortName);
            viewModel.SelectedTime = model.DateTime;
            viewModel.TargetAdress = model.TargetAdress;
            viewModel.IsTargetAdressMine = model.IsTargetAdressMine;
            viewModel.IsOriginAdressMine = model.IsOriginAdressMine;
            viewModel.Note = model.Note;

            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success)
            {
                var depositWithdrawl = DepositWithdrawls.First(x => x.Id == model.Id);
                depositWithdrawl.Fees = double.Parse(viewModel.Fees);
                depositWithdrawl.Amount = double.Parse(viewModel.Amount);
                depositWithdrawl.Currency = viewModel.SelectedCurrency;
                depositWithdrawl.DateTime = DateTimeHelper.DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime);
                depositWithdrawl.OriginAdress = viewModel.OriginAdress;
                depositWithdrawl.OriginPlatform = viewModel.SelectedOriginPlatform;
                depositWithdrawl.TargetAdress = viewModel.TargetAdress;
                depositWithdrawl.TargetPlatform = viewModel.SelectedTargetPlatform;
                depositWithdrawl.IsTargetAdressMine = viewModel.IsTargetAdressMine;
                depositWithdrawl.IsOriginAdressMine = viewModel.IsOriginAdressMine;
                depositWithdrawl.Note = viewModel.Note;

                DepositWithdrawService.Instance.Update(DepositWithdrawls);
                DepositWithdrawls = new ObservableCollection<DepositWithdrawlModel>(DepositWithdrawls.ToArray());
            }
        }

        private void ExecuteDeleteDepositWithdrawlCommand(object depositWithdrawlModel)
        {
            DepositWithdrawls.Remove((DepositWithdrawlModel)depositWithdrawlModel);
            DepositWithdrawService.Instance.Update(DepositWithdrawls);
        }

        private async void ExecuteAddDepositWithdrawl()
        {
            var dialog = new DepositWithdrawlDialog();
            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success &&
                dialog.DataContext is DepositWithdrawlDialogViewModel viewModel)
            {
                DepositWithdrawls.Add(new DepositWithdrawlModel
                {
                    Id = Guid.NewGuid(),
                    Fees = double.Parse(viewModel.Fees),
                    OriginAdress = viewModel.OriginAdress,
                    Amount = double.Parse(viewModel.Amount),
                    TargetAdress = viewModel.TargetAdress,
                    Currency = viewModel.SelectedCurrency,
                    OriginPlatform = viewModel.SelectedOriginPlatform,
                    TargetPlatform = viewModel.SelectedTargetPlatform,
                    DateTime = DateTimeHelper.DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime),
                    IsTargetAdressMine = viewModel.IsTargetAdressMine,
                    IsOriginAdressMine = viewModel.IsOriginAdressMine,
                    Note = viewModel.Note
                });

                DepositWithdrawService.Instance.Update(DepositWithdrawls);
            }
        }

    }
}
