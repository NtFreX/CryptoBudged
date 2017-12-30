using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Factories;
using CryptoBudged.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views.Widgets
{
    public class ProfitsInTotalWidgetViewModel : BindableBase
    {
        private readonly Dispatcher _dispatcher;

        private DateTime _lastPrintProgressTime = DateTime.MinValue;
        private string _lastMessage = string.Empty;

        private string _investment;
        private string _profit;
        private string _profitInPercent;
        private List<string> _profits;

        public List<string> Profits
        {
            get => _profits;
            set => SetProperty(ref _profits, value);
        }
        public string ProfitInPercent
        {
            get => _profitInPercent;
            set => SetProperty(ref _profitInPercent, value);
        }
        public string Profit
        {
            get => _profit;
            set => SetProperty(ref _profit, value);
        }
        public string Investment
        {
            get => _investment;
            set => SetProperty(ref _investment, value);
        }


        public DelegateCommand RefreshChartCommand { get; }

        public ProfitsInTotalWidgetViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            Profits = new List<string>();

            RefreshChartCommand = new DelegateCommand(ExecuteRefreshChartCommand);
            
            Task.Run(CalculateAsync);
        }

        private void ExecuteRefreshChartCommand()
        {
            Profit = "";
            ProfitInPercent = "";
            Investment = "";
            Profits = new List<string>();

            Task.Run(CalculateAsync);
        }

        private async Task CalculateAsync()
        {
            try
            {
                var allUsedCurrencies = CryptoCurrencyService.Instance.GetAllUsedCurrencies();
                var currencyIndex = 0;
                var currencyCount = allUsedCurrencies.Count();

                void ProgressCallback(string message, double progress)
                {
                    if (progress == 100.0 || _lastPrintProgressTime <= DateTime.Now - TimeSpan.FromSeconds(1) || _lastMessage != message)
                    {
                        var p = (100.0 / currencyCount * currencyIndex) + (progress / currencyCount);

                        _dispatcher.Invoke(() =>
                        {
                            Investment = "?";
                            Profit = "?";
                            ProfitInPercent = "?";
                            Profits = new List<string>(new[] {$"{message} - {p.ToString("0.00")}%"});
                        });

                        _lastPrintProgressTime = DateTime.Now;
                        _lastMessage = message;
                    }
                }
                
                var newProfits = new List<(string Text, double Percent)>();
                var allHoldings = HoldingsService.Instance.CalculateHoldings();

                var profit = 0.0;
                var totalInvestment = 0.0;
                foreach (var currency in allUsedCurrencies)
                {
                    ////if (!currency.IsCryptoCurrency)
                    ////    continue;
                    //var profitForCurrency = await CryptoCurrencyService.Instance.TryCalculateProfitAsync(
                    //    currency, 
                    //    CurrencyFactory.Instance.GetByShortName("CHF"),
                    //    ProgressCallback);
                    var investmentInCurency = await CryptoCurrencyService.Instance.CalculateInvestmentAsync(currency, CurrencyFactory.Instance.GetByShortName("CHF"));// holdingsInCurrentCurrency - profitForCurrency;
                    //var holdingInCurrency = allHoldings.FirstOrDefault(x => x.Currency.ShortName == currency.ShortName)?.Amount ?? 0.0;
                    //var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(
                    //    currency,
                    //    CurrencyFactory.Instance.GetByShortName("USD"),
                    //    holdingInCurrency,
                    //    DateTime.Now,
                    //    (_, __) => { });
                    //var inChf = await CurrencyExchangeFactory.Instance.ConvertAsync(
                    //    CurrencyFactory.Instance.GetByShortName("USD"),
                    //    CurrencyFactory.Instance.GetByShortName("CHF"),
                    //    inUsd,
                    //    DateTime.Now,
                    //    (_, __) => { });
                    var holdingInCurrency = allHoldings.FirstOrDefault(x => x.Currency.ShortName == currency.ShortName)?.AmountInChf ?? 0.0;
                    var profitForCurrency = holdingInCurrency - investmentInCurency;

                    currencyIndex++;
                    //if (!profitResult.Success)
                    //{
                    //    _dispatcher.Invoke(() =>
                    //    {
                    //        Investment = "?";
                    //        Profit = "?";
                    //        ProfitInPercent = "?";
                    //        Profits = new List<string>(new[] { profitResult.Error, profitResult.Exception?.Message });
                    //    });
                    //    return;
                    //}

                    //if (currency.IsCryptoCurrency)
                    //{
                        //var holdingsInCurrency = HoldingsService.Instance.CalculateHoldings();
                        //var holdingsInCurrentCurrency =
                        //    holdingsInCurrency.FirstOrDefault(x => x.Currency.ShortName == currency.ShortName)?.AmountInChf ?? 0;
                        var profitInPercent = holdingInCurrency / (investmentInCurency / 100.0) - 100.0;
                        var profitInPercentText = profitInPercent.ToString("0.00");
                    
                        var movementTranslation = profitForCurrency >= 0 ? "up" : "down";
                        newProfits.Add(($"{currency} : {movementTranslation} {profitForCurrency} = {profitInPercentText}%", profitInPercent));
                    //}
                    profit += profitForCurrency;
                    totalInvestment += investmentInCurency;
                }
                var holdings = allHoldings.Sum(x => x.AmountInChf);

                //var result = await CryptoCurrencyService.Instance.GetInvestmentAsync("CHF");
                //if (!result.Success)
                //    return;
                //var investment = result.Result;
                //var investment = holdings - profit;
                var investment = totalInvestment;

                _dispatcher.Invoke(() =>
                {
                    Profits = newProfits.OrderByDescending(x => x.Percent).Select(x => x.Text).ToList();
                    Investment = investment.ToString();
                    Profit = profit.ToString();
                    ProfitInPercent = (holdings / (investment / 100.0) - 100).ToString();
                });
            }
            catch (Exception exce)
            {
                _dispatcher.Invoke(() =>
                {
                    Investment = "?";
                    Profit = "?";
                    ProfitInPercent = "?";
                    Profits = new List<string>(new[] { exce.Message });
                });
            }
        }
    }
}
