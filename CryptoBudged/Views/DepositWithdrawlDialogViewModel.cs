using System;
using System.Collections.Generic;
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
    public class DepositWithdrawlDialogViewModel : BindableBase
    {
        private string _validationMessages;

        public IEnumerable<ExchangePlatformModel> TargetPlatform { get; } = ExchangePlatformFactory.Instance.GetAll();
        public IEnumerable<ExchangePlatformModel> OriginPlatform { get; } = ExchangePlatformFactory.Instance.GetAll();
        public IEnumerable<CurrencyModel> Currencies { get; } = CurrencyFactory.Instance.GetAll();
        public CurrencyModel SelectedCurrency { get; set; }
        public ExchangePlatformModel SelectedOriginPlatform { get; set; }
        public ExchangePlatformModel SelectedTargetPlatform { get; set; }
        public string OriginAdress { get; set; }
        public string Amount { get; set; }
        public string Fees { get; set; }
        public string TargetAdress { get; set; }
        public DateTime? SelectedTime { get; set; }
        public DateTime? SelectedDate { get; set; }
        public bool WithDrawFromHoldings { get; set; } = true;

        public string ValidationMessages
        {
            get => _validationMessages;
            set => SetProperty(ref _validationMessages, value);
        }

        public DelegateCommand CancleCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public DepositWithdrawlDialogViewModel()
        {
            CancleCommand = new DelegateCommand(ExecuteCancleCommand);
            SaveCommand = new DelegateCommand(ExecuteSaveCommand);

            Fees = "0";
        }

        private void ExecuteSaveCommand()
        {
            var validationMessages = new List<string>();

            if (SelectedCurrency == null)
            {
                validationMessages.Add("Select a currency");
            }
            if (SelectedOriginPlatform == null)
            {
                validationMessages.Add("Select an orgin platform");
            }
            if (string.IsNullOrEmpty(OriginAdress))
            {
                validationMessages.Add("Enter a valid origin adress");
            }
            if (SelectedTargetPlatform == null)
            {
                validationMessages.Add("Select a target platform");
            }
            if (string.IsNullOrEmpty(TargetAdress))
            {
                validationMessages.Add("Enter a valid target adress");
            }
            if (!double.TryParse(Amount, out var amount) || amount <= 0)
            {
                validationMessages.Add("Enter an amount");
            }
            if (!double.TryParse(Fees, out var fees) || fees < 0)
            {
                validationMessages.Add("Enter the fees");
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
