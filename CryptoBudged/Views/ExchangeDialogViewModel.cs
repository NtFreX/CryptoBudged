using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views
{
    public class ExchangeDialogViewModel : BindableBase
    {
        private string _originAmount;
        private string _targetAmount;
        private string _exchangeRate;
        private string _fees;
        private string _validationMessages;

        public IEnumerable<ExchangePlatformModel> ExchangePlatforms { get; } = ExchangePlatformFactory.Instance.GetCryptoCurrencyExchanges();
        public IEnumerable<CurrencyModel> TargetCurrencies { get; } = CurrencyFactory.Instance.GetAll();
        public IEnumerable<CurrencyModel> OriginCurrencies { get; } = CurrencyFactory.Instance.GetAll();
        public ExchangePlatformModel SelectedExchangePlatform { get; set; }
        public CurrencyModel SelectedTargetCurrency { get; set; }
        public CurrencyModel SelectedOriginCurrency { get; set; }
        public DateTime? SelectedTime { get; set; }
        public DateTime? SelectedDate { get; set; }

        public string ValidationMessages
        {
            get => _validationMessages;
            set => SetProperty(ref _validationMessages, value);
        }

        public string OriginAmount
        {
            get => _originAmount;
            set => SetProperty(ref _originAmount, value);
        }

        public string TargetAmount
        {
            get => _targetAmount;
            set => SetProperty(ref _targetAmount, value);
        }

        public string ExchangeRate
        {
            get => _exchangeRate;
            set => SetProperty(ref _exchangeRate, value);
        }

        public string Fees
        {
            get => _fees;
            set => SetProperty(ref _fees, value);
        }

        public DelegateCommand CanclceCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public ExchangeDialogViewModel()
        {
            CanclceCommand = new DelegateCommand(ExecuteCancleCommand);
            SaveCommand = new DelegateCommand(ExecuteSaveCommand);

            Fees = "0";
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            var isOriginAmountValid = double.TryParse(OriginAmount, out var originAmount);
            var isExchangeRateValid = double.TryParse(ExchangeRate, out var exchangeRate);

            if ((propertyName == nameof(OriginAmount) || propertyName == nameof(ExchangeRate) || propertyName == nameof(Fees)) &&
                isOriginAmountValid &&
                isExchangeRateValid && exchangeRate != 0)
            {
                var fees = double.TryParse(Fees, out var value) ? value : 0.0d;
                TargetAmount = (originAmount / exchangeRate - fees).ToString(CultureInfo.CurrentUICulture);
            }
        }

        private void ExecuteSaveCommand()
        {
            var validationMessages = new List<string>();

            if (SelectedOriginCurrency == null)
            {
                validationMessages.Add("Select a origin currency");
            }
            if (!double.TryParse(OriginAmount, out var originAmount) || originAmount <= 0)
            {
                validationMessages.Add("Enter a valid origin amount");
            }
            if (!double.TryParse(ExchangeRate, out var exchangeRate) || exchangeRate <= 0)
            {
                validationMessages.Add("Enter a valid exchange rate");
            }
            if (SelectedTargetCurrency == null)
            {
                validationMessages.Add("Select a target currency");
            }
            if (!double.TryParse(Fees, out var fees) || fees < 0)
            {
                validationMessages.Add("Enter a valid fee amount");
            }
            if (SelectedExchangePlatform == null)
            {
                validationMessages.Add("Select a exchange platform");
            }

            if (validationMessages.Count == 0)
            {
                DialogHost.CloseDialogCommand.Execute(true, null);
            }
            else
            {
                ValidationMessages = string.Join(Environment.NewLine, validationMessages);
            }
        }

        private void ExecuteCancleCommand()
            => DialogHost.CloseDialogCommand.Execute(false, null);
    }
}
