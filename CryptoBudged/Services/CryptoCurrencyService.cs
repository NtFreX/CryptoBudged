using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoBudged.Services
{
    public class CryptoCurrencyService
    {
        private readonly List<(string Currency, string FilePath)> _avaiableExchangeRates = new List<(string Currency, string FilePath)>(new []
        {
            ("BTC", @"Seed\coinbaseUSD.csv"),
            ("ETH", @"Seed\export-EtherPrice.csv")
        });

        private CryptoCurrencyService() { }

        public async Task<PriceModel> GetCurrentPricesAsync(string currencyShortName)
        {
            var content = await HttpCachingService.Instance.GetStringAsync($"https://min-api.cryptocompare.com/data/price?fsym={currencyShortName}&tsyms=BTC,USD,EUR,CHF,ETH", TimeSpan.FromSeconds(4));
            return JsonConvert.DeserializeObject<PriceModel>(content);
        }
        
        //public async void Test()
        //{
        //    // should this work with fiat currencies??
        //    var exchanges = ExchangesService.Instance.GetAll();
        //    var depositWithdraws = DepositWithdrawService.Instance.GetAll();
        //    var allUsedCurrencies = exchanges.Select(x => x.OriginCurrency).Concat(
        //            exchanges.Select(x => x.TargetCurrency)).Concat(
        //            depositWithdraws.Select(x => x.Currency))
        //            .Where(x => x.IsCryptoCurrency)
        //            .Select(x => x.ShortName)
        //        .Distinct(StringComparer.CurrentCulture);

        //    var profit = 0.0;
        //    foreach (var currency in allUsedCurrencies)
        //    {
        //        var profitResult = await TryGetProfitAsync(currency, "CHF",
        //            exchanges.ToList(),
        //            depositWithdraws.ToList());
        //        if (!profitResult.Success)
        //        {
                    
        //        }
        //        profit += profitResult.Result;
        //    }
        //    var start = HoldingsService.Instance.CalculateHoldings().Sum(x => x.AmountInChf) - profit;

        //    var trxProfit = await TryGetProfitAsync("TRX", "CHF", 
        //        ExchangesService.Instance.GetAll().ToList(),
        //        DepositWithdrawService.Instance.GetAll().ToList());

        //    var btcProfit = await TryGetProfitAsync("BTC", "CHF",
        //        ExchangesService.Instance.GetAll().ToList(),
        //        DepositWithdrawService.Instance.GetAll().ToList());

        //    var xvgProfit = await TryGetProfitAsync("XVG", "CHF",
        //        ExchangesService.Instance.GetAll().ToList(),
        //        DepositWithdrawService.Instance.GetAll().ToList());

        //    //TODO: make it work with eth
        //    var ethProfit = await TryGetProfitAsync("ETH", "CHF",
        //        ExchangesService.Instance.GetAll().ToList(),
        //        DepositWithdrawService.Instance.GetAll().ToList());
        //}
        public IEnumerable<CurrencyModel> GetAllUsedCurrencies()
        {
            var exchanges = ExchangesService.Instance.GetAll();
            var depositWithdraws = DepositWithdrawService.Instance.GetAll();
            var allUsedCurrencies = exchanges.Select(x => x.OriginCurrency).Concat(
                    exchanges.Select(x => x.TargetCurrency)).Concat(
                    depositWithdraws.Select(x => x.Currency))
                .Distinct(new CurrencyModelEqualityComparer());
            return allUsedCurrencies;
        }

        //public async Task<TryResult<double>> GetInvestmentAsync(string targetCurrencyShortName)
        //{
        //    var investment = 0.0;
        //    foreach (var depositWithdraw in DepositWithdrawService.Instance.GetAll())
        //    {
        //        if (!depositWithdraw.IsOriginAdressMine && depositWithdraw.IsTargetAdressMine)
        //        {
        //            if (depositWithdraw.Currency.IsCryptoCurrency)
        //            {
        //                if (!AreExchangeRatesAvaiable(depositWithdraw.Currency.ShortName))
        //                {
        //                    return new TryResult<double>
        //                    {
        //                        Error = $"No exchange for {depositWithdraw.Currency} exists"
        //                    };
        //                }

        //                var priceInUsd = GetNearestPriceInUsd(depositWithdraw.Currency.ShortName, depositWithdraw.DateTime) * depositWithdraw.Amount;
        //                var result = await TryConvertFiatCurrencyAsync("USD", targetCurrencyShortName, priceInUsd, depositWithdraw.DateTime);
        //                if (!result.Success)
        //                    return result;
        //                investment += result.Result;
        //            }
        //            else
        //            {
        //                var result = await TryConvertFiatCurrencyAsync(depositWithdraw.Currency.ShortName, targetCurrencyShortName, depositWithdraw.Amount, depositWithdraw.DateTime);
        //                if (!result.Success)
        //                    return result;
        //                investment += result.Result;
        //            }
        //        }
        //    }
        //    return new TryResult<double>
        //    {
        //        Success = true,
        //        Result = investment
        //    };
        //}

        //public List<(CurrencyModel currency, double)> GetProfits()
        //    => throw new NotImplementedException();
        public async Task<double> CalculateInvestmentAsync(CurrencyModel currency, CurrencyModel targetCurrency)
        {
            var usdCurrency = CurrencyFactory.Instance.GetByShortName("USD");
            var movements = DepositWithdrawService.Instance.GetAll()
                .Where(x => x.Currency.ShortName == currency.ShortName)
                .Where(x => !x.IsOriginAdressMine && x.IsTargetAdressMine);

            var investment = 0.0;
            foreach (var movment in movements)
            {
                if (movment.Currency.ShortName == targetCurrency.ShortName)
                {
                    investment += movment.Amount;
                }
                else if (movment.Currency.ShortName == usdCurrency.ShortName)
                {
                    investment += await CurrencyExchangeFactory.Instance.ConvertAsync(movment.Currency, targetCurrency, movment.Amount, movment.DateTime);
                }
                else
                {
                    var investmentInUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(currency, usdCurrency, movment.Amount, movment.DateTime);
                    investment += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, investmentInUsd, movment.DateTime);
                }
            }

            var buyAmount = await CalculateBuyAmountAsync(currency, targetCurrency);
            var sellAmount = await CalculateSellAmountAsync(currency, targetCurrency);

            return investment - sellAmount.Income + buyAmount.Coast;
        }

        public async Task<(double Coast, double Amount)> CalculateBuyAmountAsync(CurrencyModel currency, CurrencyModel targetCurrency)
        {
            var usdCurrency = CurrencyFactory.Instance.GetByShortName("USD");
            var exchanges = ExchangesService.Instance.GetAll();
            var coast = 0.0;
            var holdings = 0.0;
            foreach (var exchange in exchanges)
            {
                if (exchange.TargetCurrency.ShortName == currency.ShortName)
                {
                    // buy
                    if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.OriginCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.OriginCurrency, usdCurrency, exchange.OriginAmount, exchange.DateTime);
                        coast += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.TargetCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.TargetCurrency, usdCurrency, exchange.TargetAmount + exchange.Fees, exchange.DateTime);
                        coast += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else
                    {
                        // the profit cannot be calculated because no exchange rates exits
                        throw new ArgumentException($"No exchange rate for {exchange.OriginCurrency} exists");
                    }

                    holdings += exchange.TargetAmount;
                }
            }
            return (Coast: coast, Amount: holdings);
        }

        public async Task<(double Income, double Amount)> CalculateSellAmountAsync(CurrencyModel currency, CurrencyModel targetCurrency)
        {
            var exchanges = ExchangesService.Instance.GetAll();
            var usdCurrency = CurrencyFactory.Instance.GetByShortName("USD");
            var income = 0.0;
            var amount = 0.0;
            foreach (var exchange in exchanges)
            {
                if (exchange.OriginCurrency.ShortName == currency.ShortName)
                {
                    // sell
                    if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.TargetCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.TargetCurrency, usdCurrency, exchange.TargetAmount, exchange.DateTime);
                        income += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else if (await CurrencyExchangeFactory.Instance.CanConvertAsync(exchange.OriginCurrency, usdCurrency))
                    {
                        var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(exchange.OriginCurrency, usdCurrency, exchange.OriginAmount, exchange.DateTime);
                        income += await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, inUsd, exchange.DateTime);
                    }
                    else
                    {
                        // the profit cannot be calculated because no exchange rates exits
                        throw new ArgumentException($"No exchange rate for {exchange.OriginCurrency} exists");
                    }

                    amount += exchange.OriginAmount;
                }
            }
            return (Income: income, Amount: amount);
        }

        public async Task<double> TryCalculateProfitAsync(CurrencyModel currency, CurrencyModel targetCurrency, Action<string, double> progressCallback)
        {
            List<DepositWithdrawlModel> depositWithdrawls = DepositWithdrawService.Instance.GetAll().ToList();
            var coastAndIncome = 0.0;
            var holdings = 0.0;
            var exchangeIndex = 0;
            
            var buys = await CalculateBuyAmountAsync(currency, targetCurrency);
            var sells = await CalculateSellAmountAsync(currency, targetCurrency);

            holdings += buys.Amount - sells.Amount;
            coastAndIncome += sells.Income - buys.Coast;

            var payIns = 0.0;
            foreach (var depositWithdraw in depositWithdrawls)
            {
                if (depositWithdraw.Currency.ShortName != currency.ShortName)
                    continue;
                
                if (!depositWithdraw.IsOriginAdressMine && depositWithdraw.IsTargetAdressMine)
                {
                    // deposit
                    //TODO: ??? count this as investment thats why its not on the list???
                    holdings += depositWithdraw.Amount - depositWithdraw.Fees;

                    var inUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(
                        depositWithdraw.Currency,
                        CurrencyFactory.Instance.GetByShortName("USD"),
                        depositWithdraw.Amount,
                        depositWithdraw.DateTime);
                    payIns += await CurrencyExchangeFactory.Instance.ConvertAsync(
                        CurrencyFactory.Instance.GetByShortName("USD"),
                        targetCurrency,
                        inUsd,
                        depositWithdraw.DateTime);
                    //if (depositWithdraw.Currency.IsCryptoCurrency)
                    //{
                    //    if (!AreExchangeRatesAvaiable(depositWithdraw.Currency.ShortName))
                    //    {
                    //        // the profit cannot be calculated because no exchange rates exits
                    //        return new TryResult<double>
                    //        {
                    //            Error = $"No exchange rate for {depositWithdraw.Currency} exists"
                    //        };
                    //    }

                    //    var investmentInUsd = GetNearestPriceInUsd(depositWithdraw.Currency.ShortName, depositWithdraw.DateTime) * depositWithdraw.Amount;
                    //    var convertResult = await TryConvertFiatCurrencyAsync("USD", targetCurrencyShortName, investmentInUsd, depositWithdraw.DateTime);
                    //    if (!convertResult.Success)
                    //        return convertResult;
                    //    coastAndIncome -= convertResult.Result;
                    //}
                    //else
                    //{
                    //    var convertResult = await TryConvertFiatCurrencyAsync(depositWithdraw.Currency.ShortName, targetCurrencyShortName, depositWithdraw.Amount, depositWithdraw.DateTime);
                    //    if (!convertResult.Success)
                    //        return convertResult;
                    //    coastAndIncome -= convertResult.Result;
                    //}
                }
                else if (depositWithdraw.IsOriginAdressMine && depositWithdraw.IsTargetAdressMine)
                {
                    // move => only subtract the fees
                    holdings -= depositWithdraw.Fees;
                }
                else if (depositWithdraw.IsOriginAdressMine && !depositWithdraw.IsTargetAdressMine)
                {
                    // widthdraw


                    holdings -= depositWithdraw.Amount + depositWithdraw.Fees;
                }
            }

            // TODO: make this work with the CurrencyExchangeService
            //var currentHoldingsInUsd = await CurrencyExchangeFactory.Instance.ConvertAsync(currency, usdCurrency, holdings, DateTime.Now, ProgressCallback);
            //exchangeIndex++;
            //var currentHoldings = await CurrencyExchangeFactory.Instance.ConvertAsync(usdCurrency, targetCurrency, currentHoldingsInUsd, DateTime.Now, ProgressCallback);

            var holdingsModel = HoldingsService.Instance.CalculateHoldings()
                .FirstOrDefault(x => x.Currency.ShortName == currency.ShortName);
            var currentPrices = await GetCurrentPricesAsync(currency.ShortName);
            var currentHoldings = currentPrices.CHF * holdingsModel?.Amount ?? 0;//holdings;


            //var currentPriceInUsd = await GetCurrentPriceInUsdAsync(currencyShortName) * holdings;
            //var fiatResult = await TryConvertFiatCurrencyAsync("USD", targetCurrencyShortName, currentPriceInUsd, DateTime.Now);
            //if (!fiatResult.Success)
            //    return fiatResult;
            var investment = await CalculateInvestmentAsync(currency, targetCurrency);
            return coastAndIncome + currentHoldings; //- investment - payIns;
        }

        //private double GetNearestPriceInUsd(string currencyShortName, DateTime dateTime)
        //{
        //    if (!AreExchangeRatesAvaiable(currencyShortName))
        //    {
        //        throw new ArgumentException();
        //    }

        //    var filePath = _avaiableExchangeRates.First(x => x.Currency == currencyShortName).FilePath;
        //    var indexesFilePath = filePath + ".indices";

        //    long startingPoint = 0;
        //    if (File.Exists(indexesFilePath))
        //    {
        //        var fileContent = File.ReadAllText(indexesFilePath);
        //        var indexes = JsonConvert.DeserializeObject<List<YearIndex>>(fileContent);

        //        var theHourBefore = dateTime.AddHours(-1);

        //        var yearIndex = indexes.FirstOrDefault(x => x.Year == theHourBefore.Year);
        //        var monthIndex = yearIndex?.MonthIndices.FirstOrDefault(x => x.Month == theHourBefore.Month);
        //        var dayIndex = monthIndex?.DayIndices.FirstOrDefault(x => x.Day == theHourBefore.Day);
        //        var hourIndex = dayIndex?.HourIndices.FirstOrDefault(x => x.Hour == theHourBefore.Hour);
        //        startingPoint = hourIndex?.Position ?? dayIndex?.Position ?? monthIndex?.Position ?? yearIndex?.Position ?? 0;
        //    }

        //    var timestamp = ToUnixTimestamp(dateTime);
        //    (int TimeStamp, double Amount)? lastPrice = null;
        //    using (var stream = new StreamReader(filePath))
        //    using (var reader = new CsvReader(stream))
        //    {
        //        stream.BaseStream.Position = startingPoint;
        //        reader.Read();

        //        while (reader.Read())
        //        {
        //            var currentTimeStamp = reader.GetField<long>(0);
        //            if (currentTimeStamp > timestamp)
        //            {
        //                if (lastPrice == null)
        //                {
        //                    return reader.GetField<double>(1);
        //                }
        //                if (currentTimeStamp - timestamp < timestamp - lastPrice.Value.TimeStamp)
        //                {
        //                    return reader.GetField<double>(1);
        //                }
        //                else
        //                {
        //                    return lastPrice.Value.Amount;
        //                }
        //            }

        //            lastPrice = (TimeStamp: reader.GetField<int>(0), Amount: reader.GetField<double>(1));
        //        }
        //    }

        //    if(lastPrice != null)
        //        return lastPrice.Value.Amount;

        //    throw new ArgumentException();
        //}
        //private async Task<TryResult<double>> TryConvertFiatCurrencyAsync(string originCurrency, string targetCurrency, double amount, DateTime dateTime)
        //{
        //    try
        //    {
        //        var content = await HttpCachingService.Instance.GetStringAsync($"https://api.fixer.io/{FormatDate(dateTime)}?base={originCurrency}", TimeSpan.MaxValue);
        //        dynamic obj = JObject.Parse(content);
        //        if (!(obj.rates is JObject rates))
        //            throw new Exception();
        //        var value = double.Parse(rates.Property(targetCurrency).Value.ToString());
        //        return new TryResult<double>
        //        {
        //            Result = amount * value,
        //            Success = true
        //        };
        //    }
        //    catch (Exception exce)
        //    {
        //        return new TryResult<double>
        //        {
        //            Error = $"Converting '{amount}' of {originCurrency} to fiat currency {targetCurrency} on {dateTime} failed",
        //            Exception = exce
        //        };
        //    }
        //}
        //private async Task<double> GetCurrentPriceInUsdAsync(string currencyName)
        //    => (await GetCurrentPrices(currencyName)).USD;
        //private Int32 ToUnixTimestamp(DateTime dateTime)
        //    => (Int32)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        //private bool AreExchangeRatesAvaiable(string currency)
        //    => _avaiableExchangeRates.Any(x => x.Currency == currency);
        //private string FormatDate(DateTime dateTime)
        //    => $"{dateTime.Year:0000}-{dateTime.Month:00}-{dateTime.Day:00}";
        public static CryptoCurrencyService Instance { get; } = new CryptoCurrencyService();

        //public class TryResult<T>
        //{
        //    public bool Success { get; set; }
        //    public T Result { get; set; }
        //    public string Error { get; set; }
        //    public Exception Exception { get; set; }
        //}

        //private class YearIndex
        //{
        //    public int Year { get; set; } = 0;
        //    public long Position { get; set; } = 0;
        //    public List<MonthIndex> MonthIndices { get; set; } = new List<MonthIndex>();

        //    public class MonthIndex
        //    {
        //        public int Month { get; set; } = 0;
        //        public long Position { get; set; } = 0;
        //        public List<DayIndex> DayIndices { get; set; } = new List<DayIndex>();

        //        public class DayIndex
        //        {
        //            public int Day { get; set; } = 0;
        //            public long Position { get; set; } = 0;
        //            public List<HourIndex> HourIndices { get; set; } = new List<HourIndex>();

        //            public class HourIndex
        //            {
        //                public int Hour { get; set; } = 0;
        //                public long Position { get; set; } = 0;
        //            }
        //        }
        //    }
        //}
    }
}
