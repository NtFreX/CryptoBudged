using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptoBudged.Models;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Mvvm;

namespace CryptoBudged.Views
{
    public class MainWindowViewModel : BindableBase
    {
        /*private readonly Dictionary<string, Func<DepositWithdrawlModel, object>> _depositWithdrawOrderFuncs = new Dictionary<string, Func<DepositWithdrawlModel, object>>
        {
            { "Origin adress", x => x.OriginAdress },
            { "Origin Platform", x => x.OriginPlatform.ToString() },
            { "Target adress", x => x.TargetAdress },
            { "Target Platform", x => x.TargetPlatform.ToString() },
            { "Amount", x => x.Amount },
            { "Currency", x => x.Currency.ToString() },
            { "Fees", x => x.Fees },
            { "Withdraw from holdings", x => x.WithDrawFromHoldings },
            { "Date/Time", x => x.DateTime }
        };
        private readonly Dictionary<string, Func<ExchangeModel, object>> _exchangeOrderFuncs = new Dictionary<string, Func<ExchangeModel, object>>
        {
            { "Date/Time", x => x.DateTime },
            { "Exchange platform", x => x.ExchangePlatform.ToString() },
            { "Fees", x => x.Fees },
            { "Target amount", x => x.TargetAmount },
            { "Target currency", x => x.TargetCurrency.ToString() },
            { "Exchange rate", x => x.ExchangeRate },
            { "Origin amount", x => x.OriginAmount },
            { "Origin currency", x => x.OriginCurrency.ToString() }
        };
        private readonly Dictionary<string, Func<HoldingModel, object>> _holdingOrderFuncs = new Dictionary<string, Func<HoldingModel, object>>
        {
            { "Currency", x => x.Currency.ToString() },
            { "Amount", x => x.Amount },
            { "Amount in CHF", x => x.AmountInChf },
            { "Price in CHF", x => x.PriceInChf },
            { "Amount in BTC", x => x.AmountInBtc },
            { "Price in BTC", x => x.PriceInBtc },
            { "Amount in ETH", x => x.AmountInEth },
            { "Price in ETH", x => x.PriceInEth }
        };*/


        public MainWindowViewModel()
        {
            //var lines = File.ReadAllLines(@"C:\Users\ftr\Desktop\tradingPairs.txt").ToList();
            //lines.RemoveAll(x => double.TryParse(x, out var _));
            //lines = lines.Select(x => $"\"{x}\",").ToList();
            //File.WriteAllLines(@"C:\Users\ftr\Desktop\tradingPairs.txt", lines);
        }
    }
}
