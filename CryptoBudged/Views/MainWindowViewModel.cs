using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using CryptoBudged.Services;
using CsvHelper;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Newtonsoft.Json;
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
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        private Int32 ToUnixTimestamp(DateTime dateTime)
            => (Int32)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        private class YearIndex
        {
            public int Year { get; set; } = 0;
            public long Position { get; set; } = 0;
            public List<MonthIndex> MonthIndices { get; set; } = new List<MonthIndex>();

            public class MonthIndex
            {
                public int Month { get; set; } = 0;
                public long Position { get; set; } = 0;
                public List<DayIndex> DayIndices { get; set; } = new List<DayIndex>();

                public class DayIndex
                {
                    public int Day { get; set; } = 0;
                    public long Position { get; set; } = 0;
                    public List<HourIndex> HourIndices { get; set; } = new List<HourIndex>();

                    public class HourIndex
                    {
                        public int Hour { get; set; } = 0;
                        public long Position { get; set; } = 0;
                    }
                }
            }
        }

        private bool _isLoading;
        private string _loadingText;

        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public MainWindowViewModel()
        {
            //Task.Run(async () =>
            //{
            //    var service = new BitfinexCurrencyExchangeService();
            //    await service.ConvertAsync(
            //        CurrencyFactory.Instance.GetByShortName("IOT"),
            //        CurrencyFactory.Instance.GetByShortName("ETH"),
            //        100,
            //        DateTime.Now.AddMonths(-1));

            //});

            if (!CurrencyExchangeFactory.Instance.IsInitialized)
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                var lastProgressReport = DateTime.MinValue;
                void ProgressCallback(double progress)
                {
                    if (lastProgressReport <= DateTime.Now - TimeSpan.FromSeconds(1))
                    {
                        dispatcher.Invoke(() =>
                        {
                            LoadingText = $"Loading historical exchange rates {progress:0.00}%...";
                        });

                        lastProgressReport = DateTime.Now;
                    }
                }

                Task.Run(async () =>
                {
                    try
                    {
                        dispatcher.Invoke(() =>
                        {
                            IsLoading = true;
                            LoadingText = $"Loading historical exchange rates 0.00%...";
                        });

                        await CurrencyExchangeFactory.Instance.InitializeAsync(ProgressCallback);

                        dispatcher.Invoke(() => IsLoading = false);
                    }
                    catch (Exception exce)
                    {
                        dispatcher.Invoke(() =>
                        {
                            IsLoading = false;
                            LoadingText = $"Error during loading of historical exchange rates ({exce.Message})";
                        });
                    }
                });
            }
        //var directory = @"C:\Projects\SandboxProjects\CryptoBudged\CryptoBudged\Seed\";
        //var fileName = "coinbaseUSD.csv";
        //int rowIndex = 0;
        //var indexes = new List<YearIndex>();
        //var lastTimeStamp = 0;
        //var lastPosition = 0L;
        //using (var stream = new StreamReader(directory + fileName))
        //using (var reader = new CsvReader(stream))
        //{
        //    while (reader.Read())
        //    {
        //        var currentTimeStamp = reader.GetField<int>(0);
        //        var currentDateTime = UnixTimeStampToDateTime(currentTimeStamp);

        //        if (lastTimeStamp > currentTimeStamp)
        //        {

        //        }
        //        lastTimeStamp = currentTimeStamp;

        //        if (indexes.All(x => x.Year != currentDateTime.Year))
        //            indexes.Add(new YearIndex
        //            {
        //                Year = currentDateTime.Year,
        //                Position = lastPosition
        //            });

        //        var yearIndex = indexes.First(x => x.Year == currentDateTime.Year);

        //        if (yearIndex.MonthIndices.All(x => x.Month != currentDateTime.Month))
        //            yearIndex.MonthIndices.Add(new YearIndex.MonthIndex
        //            {
        //                Month = currentDateTime.Month,
        //                Position = lastPosition
        //            });

        //        var monthIndex = yearIndex.MonthIndices.First(x => x.Month == currentDateTime.Month);

        //        if (monthIndex.DayIndices.All(x => x.Day != currentDateTime.Day))
        //            monthIndex.DayIndices.Add(new YearIndex.MonthIndex.DayIndex
        //            {
        //                Day = currentDateTime.Day,
        //                Position = lastPosition
        //            });

        //        var dayIndex = monthIndex.DayIndices.First(x => x.Day == currentDateTime.Day);

        //        if (dayIndex.HourIndices.All(x => x.Hour != currentDateTime.Hour))
        //            dayIndex.HourIndices.Add(new YearIndex.MonthIndex.DayIndex.HourIndex
        //            {
        //                Hour = currentDateTime.Hour,
        //                Position = lastPosition
        //            });

        //        rowIndex++;
        //        lastPosition = stream.BaseStream.Position;
        //    }
        //}
        //File.WriteAllText(directory + fileName + ".indices", JsonConvert.SerializeObject(indexes));


        //var path = @"C:\Users\ftr\Downloads\export-EtherPrice.csv";
        //var lines = File.ReadAllLines(path).ToList();
        //lines = lines.Select(x =>
        //{
        //    var parts = x.Replace("\"", "").Split(",".ToCharArray());
        //    return parts[1] + "," + parts[2] + "," + parts[0];
        //}).ToList();
        //File.WriteAllLines(path, lines);

        //var lines = File.ReadAllLines(@"C:\Users\ftr\Desktop\tradingPairs.txt").ToList();
        //lines.RemoveAll(x => double.TryParse(x, out var _));
        //lines = lines.Select(x => $"\"{x}\",").ToList();
        //File.WriteAllLines(@"C:\Users\ftr\Desktop\tradingPairs.txt", lines);
    }
    }
}
