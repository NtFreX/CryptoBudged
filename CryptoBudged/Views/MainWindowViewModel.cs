using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using CryptoBudged.Models;
using CryptoBudged.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views
{
    public class MainWindowViewModel : BindableBase
    {
        private const string FileStorePath = "fileStore.json";
        
        private readonly SemaphoreSlim _calculateHoldingSemaphoreSlim = new SemaphoreSlim(1);
        private readonly Dispatcher _dispatcher;

        private string _depositWithdrawOrderColumn = "Date/Time";
        private ListSortDirection _depositWithdrawOrderDirection = ListSortDirection.Ascending;
        private string _exchangeOrderColumn = "Date/Time";
        private ListSortDirection _exchangeOrderDirection = ListSortDirection.Ascending;
        private string _holdingOrderColumn = "Amount in CHF";
        private ListSortDirection _holdingOrderDirection = ListSortDirection.Ascending;

        private readonly Dictionary<string, Func<DepositWithdrawlModel, object>> _depositWithdrawOrderFuncs = new Dictionary<string, Func<DepositWithdrawlModel, object>>
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
        };

        private ObservableCollection<ExchangeModel> _exchanges;
        private ObservableCollection<DepositWithdrawlModel> _depositWithdrawls;
        private ObservableCollection<HoldingModel> _holdings;
        private double _totalAmountInCHF;
        private double _totalAmountInBTC;
        private double _totalAmountInETH;
        private SeriesCollection _holdingsPieChartSeries;
        private SeriesCollection _historicalHoldingsLineChartSeries;
        private bool _hasHoldingsCalculationError;
        private bool _hasDashboardCalculationError;

        public DelegateCommand AddExchange { get; }
        public DelegateCommand AddDepositWithdrawl { get; }
        public DelegateCommand<object> DeteleExchangeCommand { get; }
        public DelegateCommand<object> DeleteDepositWithdrawlCommand { get; set; }
        public DelegateCommand<object> EditExchangeCommand { get; set; }
        public DelegateCommand<object> EditDepositWithdrawlCommand { get; set; }

        public bool HasDashboardCalculationError
        {
            get => _hasDashboardCalculationError;
            set => SetProperty(ref _hasDashboardCalculationError, value);
        }
        public bool HasHoldingsCalculationError
        {
            get => _hasHoldingsCalculationError;
            set => SetProperty(ref _hasHoldingsCalculationError, value);
        }
        public SeriesCollection HistoricalHoldingsLineChartSeries
        {
            get => _historicalHoldingsLineChartSeries;
            set => SetProperty(ref _historicalHoldingsLineChartSeries, value);
        }
        public SeriesCollection HoldingsPieChartSeries
        {
            get => _holdingsPieChartSeries;
            set => SetProperty(ref _holdingsPieChartSeries, value);
        }
        public ObservableCollection<ExchangeModel> Exchanges
        {
            get => _exchanges;
            set => SetProperty(ref _exchanges, value);
        }
        public ObservableCollection<DepositWithdrawlModel> DepositWithdrawls
        {
            get => _depositWithdrawls;
            set => SetProperty(ref _depositWithdrawls, value);
        }
        public ObservableCollection<HoldingModel> Holdings
        {
            get => _holdings;
            set => SetProperty(ref _holdings, value);
        }
        public double TotalAmountInCHF
        {
            get => _totalAmountInCHF;
            set => SetProperty(ref _totalAmountInCHF, value);
        }
        public double TotalAmountInBTC
        {
            get => _totalAmountInBTC;
            set => SetProperty(ref _totalAmountInBTC, value);
        }
        public double TotalAmountInETH
        {
            get => _totalAmountInETH;
            set => SetProperty(ref _totalAmountInETH, value);
        }


        public MainWindowViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            if (File.Exists(FileStorePath))
            {
                LoadFileStore();
                CalculateHoldings();
                CalculatePieChart();
                CalculateLineChart();
            }
            else
            {
                Exchanges = new ObservableCollection<ExchangeModel>();
                DepositWithdrawls = new ObservableCollection<DepositWithdrawlModel>();
                Holdings = new ObservableCollection<HoldingModel>();
            }

            Exchanges.CollectionChanged += FileStoreCollectionChanged;
            DepositWithdrawls.CollectionChanged += FileStoreCollectionChanged;

            AddDepositWithdrawl = new DelegateCommand(ExecuteAddDepositWithdrawl);
            AddExchange = new DelegateCommand(ExecuteAddExchangeAsync);
            DeteleExchangeCommand = new DelegateCommand<object>(ExecuteDeleteExchangeCommand);
            DeleteDepositWithdrawlCommand = new DelegateCommand<object>(ExecuteDeleteDepositWithdrawlCommand);
            EditExchangeCommand = new DelegateCommand<object>(ExecuteEditExchangeCommandAsync);
            EditDepositWithdrawlCommand = new DelegateCommand<object>(ExecuteEditDepositWithdrawlCommandAsync);
            
            Task.Run(ReloadWorker);
            Task.Run(ReloadPieCharWorker);
        }

        private async Task ReloadWorker()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(5000);
                    CalculateHoldings();
                    HasHoldingsCalculationError = false;
                }
                catch
                {
                    HasHoldingsCalculationError = true;
                }
            }
        }

        private async Task ReloadPieCharWorker()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(10000);
                    CalculatePieChart();
                    HasDashboardCalculationError = false;
                }
                catch
                {
                    HasDashboardCalculationError = true;
                }
            }
        }

        private void FileStoreCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (DepositWithdrawls == null || Exchanges == null)
                return;

            var fireStoreModel = new FileStoreModel
            {
                Exchanges = Exchanges.ToList(),
                DepositWithdrawls = DepositWithdrawls.ToList()
            };

            var json = JsonConvert.SerializeObject(fireStoreModel);
            File.WriteAllText(FileStorePath, json);

            CalculateHoldings();
            CalculatePieChart();
            CalculateLineChart();
        }

        private void CalculateLineChart()
        {
            if (DepositWithdrawls == null || Exchanges == null)
                return;

            _calculateHoldingSemaphoreSlim.Wait();

            var depositsWithdrawls = DepositWithdrawls.OrderBy(x => x.DateTime);
            var exchanges = Exchanges.OrderBy(x => x.DateTime);

            var movements = new List<KeyValuePair<DateTime, List<object>>>();
            foreach (var depositsWithdrawl in depositsWithdrawls)
            {
                if (movements.All(x => x.Key != depositsWithdrawl.DateTime))
                {
                    movements.Add(new KeyValuePair<DateTime, List<object>>(depositsWithdrawl.DateTime, new List<object>(new object[] { depositsWithdrawl })));
                }
                else
                {
                    movements.First(x => x.Key == depositsWithdrawl.DateTime).Value.Add(depositsWithdrawl);
                }
            }
            foreach (var exchange in exchanges)
            {
                if (movements.All(x => x.Key != exchange.DateTime))
                {
                    movements.Add(new KeyValuePair<DateTime, List<object>>(exchange.DateTime, new List<object>(new object[] { exchange })));
                }
                else
                {
                    movements.First(x => x.Key == exchange.DateTime).Value.Add(exchange);
                }
            }
            var holdingPoints = new List<KeyValuePair<DateTime, HoldingModel>>();
            var orderedMovements = movements.OrderBy(x => x.Key);

            //holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(orderedMovements.First().Key - new TimeSpan(0, 1, 0), new HoldingModel()));

            double GetCurrentAmount(List<KeyValuePair<DateTime, HoldingModel>> points, CurrencyModel currency)
            {
                var value = points.LastOrDefault(x => x.Value.Currency.ShortName == currency.ShortName);
                if (value.Value == null || value.Value?.Amount == 0)
                    return 0;
                return value.Value.Amount;
            }

            foreach (var movement in orderedMovements)
            {
                foreach (var movementValue in movement.Value)
                {
                    if (movementValue is DepositWithdrawlModel depositWithdrawl)
                    {
                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(depositWithdrawl.DateTime, new HoldingModel
                        {
                            Currency = depositWithdrawl.Currency,
                            Amount = GetCurrentAmount(holdingPoints, depositWithdrawl.Currency) + depositWithdrawl.Amount - depositWithdrawl.Fees
                        }));
                    }
                    else if (movementValue is ExchangeModel exchange)
                    {
                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(exchange.DateTime, new HoldingModel
                        {
                            Currency = exchange.OriginCurrency,
                            Amount = GetCurrentAmount(holdingPoints, exchange.OriginCurrency) - exchange.OriginAmount
                        }));
                        holdingPoints.Add(new KeyValuePair<DateTime, HoldingModel>(exchange.DateTime, new HoldingModel
                        {
                            Currency = exchange.TargetCurrency,
                            Amount = GetCurrentAmount(holdingPoints, exchange.TargetCurrency) + exchange.TargetAmount
                        }));
                    }
                }
            }

            var seriesCollection = new SeriesCollection();
            foreach (var currency in holdingPoints.Select(x => x.Value.Currency))
            {
                var stackedAreaSeries = new StackedAreaSeries
                {
                    Title = currency.ToString(),
                    Values = new ChartValues<DateTimePoint>()
                };
                foreach (var points in holdingPoints.Where(x => x.Value.Currency.ShortName == currency.ShortName))
                {
                    stackedAreaSeries.Values.Add(new DateTimePoint(points.Key, points.Value.Amount));
                }
                seriesCollection.Add(stackedAreaSeries);
            }
            HistoricalHoldingsLineChartSeries = seriesCollection;


            _calculateHoldingSemaphoreSlim.Release();
        }
        private void CalculatePieChart()
        {
            if (DepositWithdrawls == null || Exchanges == null)
                return;

            _calculateHoldingSemaphoreSlim.Wait();

            _dispatcher.Invoke(() =>
            {
                if (HoldingsPieChartSeries == null)
                    HoldingsPieChartSeries = new SeriesCollection();

                var series = HoldingsPieChartSeries;
                foreach (var holding in Holdings)
                {
                    if (series.Any(x => x.Title == holding.Currency.ToString()))
                    {
                        var value = series.First(x => x.Title == holding.Currency.ToString());
                        value.Values = new ChartValues<double>(new[] { holding.AmountInChf });
                    }
                    else
                    {
                        series.Add(new PieSeries
                        {
                            Values = new ChartValues<double>(new[] { holding.AmountInChf }),
                            Title = holding.Currency.ToString()
                        });
                    }
                }
            });

            _calculateHoldingSemaphoreSlim.Release();
        }
        private void CalculateHoldings()
        {
            if (DepositWithdrawls == null || Exchanges == null)
                return;

            _calculateHoldingSemaphoreSlim.Wait();

            var holdings = new List<HoldingModel>();

            foreach (var exchange in Exchanges)
            {
                if (holdings.All(x => x.Currency.ShortName != exchange.OriginCurrency.ShortName))
                {
                    holdings.Add(new HoldingModel
                    {
                        Amount = 0,
                        Currency = exchange.OriginCurrency
                    });
                }
                if (holdings.All(x => x.Currency.ShortName != exchange.TargetCurrency.ShortName))
                {
                    holdings.Add(new HoldingModel
                    {
                        Amount = 0,
                        Currency = exchange.TargetCurrency
                    });
                }

                holdings.Find(x => x.Currency.ShortName == exchange.OriginCurrency.ShortName).Amount -= exchange.OriginAmount;
                holdings.Find(x => x.Currency.ShortName == exchange.TargetCurrency.ShortName).Amount += exchange.TargetAmount;
            }

            foreach (var depositWithdrawl in DepositWithdrawls)
            {
                if (holdings.All(x => x.Currency.ShortName != depositWithdrawl.Currency.ShortName))
                {
                    holdings.Add(new HoldingModel
                    {
                        Amount = 0,
                        Currency = depositWithdrawl.Currency
                    });
                }

                holdings.Find(x => x.Currency.ShortName == depositWithdrawl.Currency.ShortName).Amount += depositWithdrawl.Amount - depositWithdrawl.Fees;

                if (depositWithdrawl.WithDrawFromHoldings)
                {
                    holdings.Find(x => x.Currency.ShortName == depositWithdrawl.Currency.ShortName).Amount -= depositWithdrawl.Amount;
                }
            }

            holdings.RemoveAll(x => x.Amount == 0);

            var cryptoCurrencyService = new CryptoCurrencyService();
            var tasks = new List<Task>();
            foreach (var holding in holdings)
            {
                var tmpHolding = holding;
                tasks.Add(Task.Run(async () =>
                {
                    var prices = await cryptoCurrencyService.GetPriceOfCurrencyAsync(holding.Currency);
                    tmpHolding.AmountInBtc = tmpHolding.Amount * prices.BTC;
                    tmpHolding.AmountInChf = tmpHolding.Amount * prices.CHF;
                    tmpHolding.AmountInEth = tmpHolding.Amount * prices.ETH;

                    tmpHolding.PriceInBtc = prices.BTC;
                    tmpHolding.PriceInChf = prices.CHF;
                    tmpHolding.PriceInEth = prices.ETH;
                }));
            }

            if (!Task.WaitAll(tasks.ToArray(), new TimeSpan(0, 1, 0)))
            {
                throw new TaskCanceledException();
            }
            
            TotalAmountInCHF = holdings.Sum(x => x.AmountInChf);
            TotalAmountInBTC = holdings.Sum(x => x.AmountInBtc);
            TotalAmountInETH = holdings.Sum(x => x.AmountInEth);
            
            SetHoldings(holdings);

            _calculateHoldingSemaphoreSlim.Release();
        }

        private void LoadFileStore()
        {
            var fileContent = File.ReadAllText(FileStorePath);
            var fileStoreModel = JsonConvert.DeserializeObject<FileStoreModel>(fileContent);

            SetExchanges(fileStoreModel.Exchanges);
            SetDepositWithdraws(fileStoreModel.DepositWithdrawls);
        }

        private DateTime DateAndTimeToDateTime(DateTime? date, DateTime? time)
        {
            var value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);

            if (date != null)
            {
                value = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
            }
            if (time != null)
            {
                value = new DateTime(value.Year, value.Month, value.Day, time.Value.Hour, time.Value.Minute, time.Value.Second);
            }

            return value;
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
            viewModel.WithDrawFromHoldings = model.WithDrawFromHoldings;
            
            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success)
            {
                var depositWithdrawl = DepositWithdrawls.First(x => x.Id == model.Id);
                depositWithdrawl.Fees = double.Parse(viewModel.Fees);
                depositWithdrawl.Amount = double.Parse(viewModel.Amount);
                depositWithdrawl.Currency = viewModel.SelectedCurrency;
                depositWithdrawl.DateTime = DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime);
                depositWithdrawl.OriginAdress = viewModel.OriginAdress;
                depositWithdrawl.OriginPlatform = viewModel.SelectedOriginPlatform;
                depositWithdrawl.TargetAdress = viewModel.TargetAdress;
                depositWithdrawl.TargetPlatform = viewModel.SelectedTargetPlatform;
                depositWithdrawl.WithDrawFromHoldings = viewModel.WithDrawFromHoldings;

                SetDepositWithdraws(DepositWithdrawls.ToArray());
            }
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
            
            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success)
            {
                var exchange = Exchanges.First(x => x.Id == model.Id);
                exchange.Fees = double.Parse(viewModel.Fees);
                exchange.DateTime = DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime);
                exchange.ExchangePlatform = viewModel.SelectedExchangePlatform;
                exchange.ExchangeRate = double.Parse(viewModel.ExchangeRate);
                exchange.OriginAmount = double.Parse(viewModel.OriginAmount);
                exchange.OriginCurrency = viewModel.SelectedOriginCurrency;
                exchange.TargetAmount = double.Parse(viewModel.TargetAmount);
                exchange.TargetCurrency = viewModel.SelectedTargetCurrency;
                
                SetExchanges(Exchanges.ToArray());
            }
        }

        private void ExecuteDeleteDepositWithdrawlCommand(object depositWithdrawlModel)
        {
            DepositWithdrawls.Remove((DepositWithdrawlModel) depositWithdrawlModel);

            FileStoreCollectionChanged(this, default(NotifyCollectionChangedEventArgs));
        }

        private void ExecuteDeleteExchangeCommand(object exchangeModel)
        {
            Exchanges.Remove((ExchangeModel) exchangeModel);

            FileStoreCollectionChanged(this, default(NotifyCollectionChangedEventArgs));
        }

        private async void ExecuteAddDepositWithdrawl()
        {
            var dialog = new DepositWithdrawlDialog();
            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success &&
                dialog.DataContext is DepositWithdrawlDialogViewModel viewModel)
            {
                var depositsWithdrawlNew = DepositWithdrawls.ToList();

                depositsWithdrawlNew.Add(new DepositWithdrawlModel
                {
                    Id = Guid.NewGuid(),
                    Fees = double.Parse(viewModel.Fees),
                    OriginAdress = viewModel.OriginAdress,
                    Amount = double.Parse(viewModel.Amount),
                    TargetAdress = viewModel.TargetAdress,
                    Currency = viewModel.SelectedCurrency,
                    OriginPlatform = viewModel.SelectedOriginPlatform,
                    TargetPlatform = viewModel.SelectedTargetPlatform,
                    DateTime = DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime),
                    WithDrawFromHoldings = viewModel.WithDrawFromHoldings
                });

                SetDepositWithdraws(depositsWithdrawlNew);
            }
        }

        private async void ExecuteAddExchangeAsync()
        {
            var dialog = new ExchangeDialog();
            var result = await DialogHost.Show(dialog);
            if (bool.TryParse(result.ToString(), out var success) && success &&
                dialog.DataContext is ExchangeDialogViewModel viewModel)
            {
                var exchangesNew = Exchanges.ToList();

                exchangesNew.Add(new ExchangeModel
                {
                    Id = Guid.NewGuid(),
                    Fees = double.Parse(viewModel.Fees),
                    ExchangeRate = double.Parse(viewModel.ExchangeRate),
                    OriginAmount = double.Parse(viewModel.OriginAmount),
                    TargetAmount = double.Parse(viewModel.TargetAmount),
                    ExchangePlatform = viewModel.SelectedExchangePlatform,
                    OriginCurrency = viewModel.SelectedOriginCurrency,
                    TargetCurrency = viewModel.SelectedTargetCurrency,
                    DateTime = DateAndTimeToDateTime(viewModel.SelectedDate, viewModel.SelectedTime)
                });

                SetExchanges(exchangesNew);
            }
        }

        private void SetExchanges(IEnumerable<ExchangeModel> exchanges)
        {
            Exchanges = new ObservableCollection<ExchangeModel>(
                _exchangeOrderDirection == ListSortDirection.Ascending
                ? exchanges.OrderBy(_exchangeOrderFuncs[_exchangeOrderColumn])
                : exchanges.OrderByDescending(_exchangeOrderFuncs[_exchangeOrderColumn]));

            FileStoreCollectionChanged(this, default(NotifyCollectionChangedEventArgs));
        }

        private void SetDepositWithdraws(IEnumerable<DepositWithdrawlModel> depositWithdrawls)
        {
            DepositWithdrawls = new ObservableCollection<DepositWithdrawlModel>(
                _depositWithdrawOrderDirection == ListSortDirection.Ascending
                ? depositWithdrawls.OrderBy(_depositWithdrawOrderFuncs[_depositWithdrawOrderColumn])
                : depositWithdrawls.OrderByDescending(_depositWithdrawOrderFuncs[_depositWithdrawOrderColumn]));

            FileStoreCollectionChanged(this, default(NotifyCollectionChangedEventArgs));
        }

        private void SetHoldings(IEnumerable<HoldingModel> holdings)
        {
            Holdings = new ObservableCollection<HoldingModel>(
                _holdingOrderDirection == ListSortDirection.Ascending
                ? holdings.OrderBy(_holdingOrderFuncs[_holdingOrderColumn])
                : holdings.OrderByDescending(_holdingOrderFuncs[_holdingOrderColumn]));
        }

        public ListSortDirection SortExchanges(GridViewColumnHeader headerClicked)
        {
            if (_exchangeOrderColumn == headerClicked.Content.ToString())
            {
                _exchangeOrderDirection = _exchangeOrderDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _exchangeOrderColumn = headerClicked.Content.ToString();
                _exchangeOrderDirection = ListSortDirection.Ascending;
            }

            SetExchanges(Exchanges.ToArray());
            return _exchangeOrderDirection;
        }

        public ListSortDirection SortDepositsWithdrawls(GridViewColumnHeader headerClicked)
        {
            if (_depositWithdrawOrderColumn == headerClicked.Content.ToString())
            {
                _depositWithdrawOrderDirection = _depositWithdrawOrderDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _depositWithdrawOrderColumn = headerClicked.Content.ToString();
                _depositWithdrawOrderDirection = ListSortDirection.Ascending;
            }

            SetDepositWithdraws(DepositWithdrawls.ToArray());
            return _depositWithdrawOrderDirection;
        }

        public ListSortDirection SortHolding(GridViewColumnHeader headerClicked)
        {
            if (_holdingOrderColumn == headerClicked.Content.ToString())
            {
                _holdingOrderDirection = _holdingOrderDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _holdingOrderColumn = headerClicked.Content.ToString();
                _holdingOrderDirection = ListSortDirection.Ascending;
            }

            SetHoldings(Holdings.ToArray());
            return _holdingOrderDirection;
        }
    }
}
