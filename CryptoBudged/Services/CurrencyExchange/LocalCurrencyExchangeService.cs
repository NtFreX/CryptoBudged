using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoBudged.Extensions;
using CryptoBudged.Factories;
using CryptoBudged.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace CryptoBudged.Services.CurrencyExchange
{
    public class LocalCurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly List<(string CurrencyOne, string CurrencyTwo, string FilePath, string SourceUri, Action<string, Action<double>> AfterDownloadAction)> _supportedExchangeRates = new List<(string CurrencyOne, string CurrencyTwo, string FilePath, string SourceUri, Action<string, Action<double>> AfterDownloadAction)>(new (string CurrencyOne, string CurrencyTwo, string FilePath, string SourceUri, Action<string, Action<double>> AfterDownloadAction)[]
        {
            (CurrencyOne: "BTC", CurrencyTwo: "USD", FilePath: @"Seed\BTC-USD.csv", SourceUri: "http://api.bitcoincharts.com/v1/csv/coinbaseUSD.csv.gz", AfterDownloadAction: AfterBtcDownloadAction),
            (CurrencyOne: "ETH", CurrencyTwo: "USD", FilePath: @"Seed\ETH-USD.csv", SourceUri: "https://etherscan.io/chart/etherprice?output=csv", AfterDownloadAction: AfterEthDownloadAction)
        });

        private static void AfterBtcDownloadAction(string filePath, Action<double> progressCallback)
        {
            using (var originalFileStream = new FileStream(filePath, FileMode.Open))
            {
                using (var fileStream = new FileStream(filePath + ".tmp", FileMode.Create))
                {
                    using (var stream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }

            if(File.Exists(filePath))
                File.Delete(filePath);
            File.Move(filePath + ".tmp", filePath);
        }
        private static void AfterEthDownloadAction(string filePath, Action<double> progressCallback)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            lines.RemoveAt(0);
            lines = lines.Select(x =>
            {
                var parts = x.Replace("\"", "").Split(",".ToCharArray());
                return parts[1] + "," + parts[2] + "," + parts[0];
            }).Where(x => !string.IsNullOrEmpty(x)).ToList();
            File.WriteAllLines(filePath, lines);
        }


        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync(Action<double> progressCallback)
        {
            CurrencyModel ToCurrency(string shortName)
            {
                return CurrencyFactory.Instance.GetByShortName(shortName);
            }

            var exchangeRateIndex = 0;
            var exchangeRateCount = _supportedExchangeRates.Count;
            void ProgressCallback(double progress)
            {
                progressCallback(progress / exchangeRateCount + (100.0 / exchangeRateCount * exchangeRateIndex));
            }

            foreach (var exchangeRate in _supportedExchangeRates)
            {
                var currencyOne = ToCurrency(exchangeRate.CurrencyOne);
                var currencyTwo = ToCurrency(exchangeRate.CurrencyTwo);

                if (!await IsExchangeRateFileLoadedAsync(currencyOne, currencyTwo))
                    await LoadExchangeRateFileAsync(currencyOne, currencyTwo, ProgressCallback);

                if(!IsIndexFileExisting(currencyOne, currencyTwo))
                    await CreateIndexFileAsync(currencyOne, currencyTwo, ProgressCallback);

                progressCallback(100.0 / exchangeRateCount * exchangeRateIndex);
                
                exchangeRateIndex++;
            }

            IsInitialized = true;
        }

        public Task<bool> CanConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
            => Task.FromResult(_supportedExchangeRates.Any(x => (x.CurrencyOne == originCurrency.ShortName || x.CurrencyOne == targetCurrency.ShortName) && (x.CurrencyTwo == originCurrency.ShortName || x.CurrencyTwo == targetCurrency.ShortName)));
        public async Task<bool> IsExchangeRateFileLoadedAsync(CurrencyModel currencyOne, CurrencyModel currencyTwo)
        {
            if (!await CanConvertAsync(currencyOne, currencyTwo))
                return false;

            var exchangeRateConfiguration = _supportedExchangeRates.First(x =>
                (x.CurrencyOne == currencyOne.ShortName || x.CurrencyTwo == currencyOne.ShortName) &&
                (x.CurrencyTwo == currencyTwo.ShortName || x.CurrencyTwo == currencyOne.ShortName));

            if (!File.Exists(exchangeRateConfiguration.FilePath))
                return false;

            var fileInfo = new FileInfo(exchangeRateConfiguration.FilePath);
            if (fileInfo.LastWriteTime.Date < DateTime.Now.Date)
                return false;

            return true;
        }
        public bool IsIndexFileExisting(CurrencyModel currencyOne, CurrencyModel currencyTwo)
        {
            var exchangeRateConfiguration = _supportedExchangeRates.First(x =>
                (x.CurrencyOne == currencyOne.ShortName || x.CurrencyTwo == currencyOne.ShortName) &&
                (x.CurrencyTwo == currencyTwo.ShortName || x.CurrencyTwo == currencyOne.ShortName));

            if (!File.Exists(exchangeRateConfiguration.FilePath))
                return false;

            var fileHash = GetChecksum(exchangeRateConfiguration.FilePath);
            return File.Exists(exchangeRateConfiguration.FilePath + $".{fileHash}.indices");
        }
        public async Task CreateIndexFileAsync(CurrencyModel currencyOne, CurrencyModel currencyTwo, Action<double> progressCallback)
        {
            if (!await IsExchangeRateFileLoadedAsync(currencyOne, currencyTwo))
                throw new ArgumentException();

            var exchangeRateConfiguration = _supportedExchangeRates.First(x =>
                (x.CurrencyOne == currencyOne.ShortName || x.CurrencyTwo == currencyOne.ShortName) &&
                (x.CurrencyTwo == currencyTwo.ShortName || x.CurrencyTwo == currencyOne.ShortName));

            var indexes = new List<YearIndex>();
            var lastPosition = 0L;
            using (var stream = new StreamReader(exchangeRateConfiguration.FilePath))
            using (var reader = new CsvReader(stream))
            {
                while (reader.Read())
                {
                    var currentTimeStamp = reader.GetField<long>(0);
                    var currentDateTime = DateTimeExtensions.UnixTimeSecondsToDateTime(currentTimeStamp);

                    if (indexes.All(x => x.Year != currentDateTime.Year))
                        indexes.Add(new YearIndex
                        {
                            Year = currentDateTime.Year,
                            Position = lastPosition
                        });

                    var yearIndex = indexes.First(x => x.Year == currentDateTime.Year);

                    if (yearIndex.MonthIndices.All(x => x.Month != currentDateTime.Month))
                        yearIndex.MonthIndices.Add(new YearIndex.MonthIndex
                        {
                            Month = currentDateTime.Month,
                            Position = lastPosition
                        });

                    var monthIndex = yearIndex.MonthIndices.First(x => x.Month == currentDateTime.Month);

                    if (monthIndex.DayIndices.All(x => x.Day != currentDateTime.Day))
                        monthIndex.DayIndices.Add(new YearIndex.MonthIndex.DayIndex
                        {
                            Day = currentDateTime.Day,
                            Position = lastPosition
                        });

                    var dayIndex = monthIndex.DayIndices.First(x => x.Day == currentDateTime.Day);

                    if (dayIndex.HourIndices.All(x => x.Hour != currentDateTime.Hour))
                        dayIndex.HourIndices.Add(new YearIndex.MonthIndex.DayIndex.HourIndex
                        {
                            Hour = currentDateTime.Hour,
                            Position = lastPosition
                        });

                    lastPosition = stream.BaseStream.Position;

                    progressCallback(100.0 / stream.BaseStream.Length * stream.BaseStream.Position);
                }
            }
            var fileHash = GetChecksum(exchangeRateConfiguration.FilePath);
            File.WriteAllText(exchangeRateConfiguration.FilePath + $".{fileHash}.indices", JsonConvert.SerializeObject(indexes));
        }
        public async Task LoadExchangeRateFileAsync(CurrencyModel currencyOne, CurrencyModel currencyTwo, Action<double> progressCallback)
        {
            if (!await CanConvertAsync(currencyOne, currencyTwo))
                throw new ArgumentException();

            var exchangeRateConfiguration = _supportedExchangeRates.First(x =>
                (x.CurrencyOne == currencyOne.ShortName || x.CurrencyTwo == currencyOne.ShortName) &&
                (x.CurrencyTwo == currencyTwo.ShortName || x.CurrencyTwo == currencyOne.ShortName));

            if (IsIndexFileExisting(currencyOne, currencyTwo))
                DeleteIndexFile(exchangeRateConfiguration.FilePath + ".indices");

            var progressIndex = 0;
            var progressCount = 2;
            void ProgressCallback(double progress)
            {
                progressCallback(100.0 / progressCount * progressIndex + progress / progressCount);
            }

            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) })
            {
                using (var stream = await httpClient.GetStreamAsync(exchangeRateConfiguration.SourceUri))
                {
                    using (var fileStream = new FileStream(exchangeRateConfiguration.FilePath, FileMode.Create))
                    {
                        byte[] buffer = new byte[16 * 1024];

                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, read);

                            if(stream.CanSeek)
                                ProgressCallback(100.0 / stream.Length * stream.Position);
                        }
                    }
                }
            }

            progressIndex++;
            
            exchangeRateConfiguration.AfterDownloadAction(exchangeRateConfiguration.FilePath, ProgressCallback);
        }
        public async Task<TimeSpan> IsRateLimitedAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, DateTime dateTime)
            => await Task.FromResult(TimeSpan.Zero);
        public async Task<double> ConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime)
        {
            if (!await CanConvertAsync(originCurrency, targetCurrency))
                throw new ArgumentException();
            if(!IsInitialized)
                throw new ArgumentException();
            if (!await IsExchangeRateFileLoadedAsync(originCurrency, targetCurrency))
                throw new ArgumentException();
            if(!IsIndexFileExisting(originCurrency, targetCurrency))
                throw new ArgumentException();
            
            var exchangeRateConfiguration = _supportedExchangeRates.First(x =>
                (x.CurrencyOne == originCurrency.ShortName || x.CurrencyTwo == originCurrency.ShortName) &&
                (x.CurrencyTwo == targetCurrency.ShortName || x.CurrencyTwo == targetCurrency.ShortName));

            var startingPoint = CalculateStartingPoint(exchangeRateConfiguration.FilePath, dateTime);
            
            var nearestExchangeRate = GetNearestExchangeRate(exchangeRateConfiguration.FilePath, startingPoint, dateTime);

            double result = 0.0;
            if (originCurrency.ShortName == exchangeRateConfiguration.CurrencyOne)
            {
                result = amount * nearestExchangeRate;
            }
            else
            {
                result = amount / nearestExchangeRate;
            }
            
            return await Task.FromResult(result);
        }

        private string GetChecksum(string file)
        {
            var fileInfo = new FileInfo(file);
            return fileInfo.LastWriteTime.ToUnixTimeSeconds().ToString();

            // calculating this ckecksum takes to long
            //using (FileStream stream = File.OpenRead(file))
            //{
            //    var sha = new SHA256Managed();
            //    byte[] checksum = sha.ComputeHash(stream);
            //    return BitConverter.ToString(checksum).Replace("-", String.Empty);
            //}
        }
        private void DeleteIndexFile(string filePath)
            => File.Delete(filePath);
        private double GetNearestExchangeRate(string exchangeRateFile, long startingPoint, DateTime dateTime)
        {
            var timestamp = dateTime.ToUnixTimeSeconds();
            (long TimeStamp, double Amount)? lastPrice = null;
            //var hasRead = false;
            //while (!hasRead)
            //{
            try
            {
                using (var stream = new StreamReader(exchangeRateFile))
                using (var reader = new CsvReader(stream, new Configuration {Delimiter = ","}))
                {
                    stream.BaseStream.Position = startingPoint;

                    while (reader.Read())
                    {
                        //hasRead = true;

                        //var isValid = false;
                        //long currentTimeStamp = 0;
                        //while (!isValid)
                        //{
                        //    var fieldOne = reader.GetField(0);
                        //    if (!(isValid = long.TryParse(fieldOne, out currentTimeStamp)))
                        //    {
                        //        //TODO: fix this bug with the btc file
                        //        if (!reader.Read())
                        //            break;
                        //    }
                        //}
                        var currentTimeStamp = reader.GetField<long>(0);
                        if (currentTimeStamp > timestamp && DateTimeExtensions.UnixTimeSecondsToDateTime(currentTimeStamp) <= DateTime.Now)
                        {
                            if (lastPrice == null)
                            {
                                return reader.GetField<double>(1);
                            }
                            if (currentTimeStamp - timestamp < timestamp - lastPrice.Value.TimeStamp)
                            {
                                return reader.GetField<double>(1);
                            }

                            return lastPrice.Value.Amount;
                        }

                        lastPrice = (TimeStamp: reader.GetField<long>(0), Amount: reader.GetField<double>(1));
                    }
                }
            }
            catch
            {
                // TODO: fix eth indexing
                if (startingPoint != 0)
                    return GetNearestExchangeRate(exchangeRateFile, 0, dateTime);
            }

            //if (!hasRead)
            //{
            //    // TODO: fix eth indexing
            //    startingPoint = 0;
            //}
            // }

            if (lastPrice != null)
                return lastPrice.Value.Amount;

            throw new Exception();
        }
        private long CalculateStartingPoint(string exchangeRateFile, DateTime dateTime)
        {
            var indexesFilePath = exchangeRateFile + ".indices";

            if (!File.Exists(indexesFilePath))
            {
                return 0;
            }

            var fileContent = File.ReadAllText(indexesFilePath);
            var indexes = JsonConvert.DeserializeObject<List<YearIndex>>(fileContent);

            var theHourBefore = dateTime.AddHours(-1);

            var yearIndex = indexes.FirstOrDefault(x => x.Year == theHourBefore.Year);
            var monthIndex = yearIndex?.MonthIndices.FirstOrDefault(x => x.Month == theHourBefore.Month);
            var dayIndex = monthIndex?.DayIndices.FirstOrDefault(x => x.Day == theHourBefore.Day);
            var hourIndex = dayIndex?.HourIndices.FirstOrDefault(x => x.Hour == theHourBefore.Hour);
            return hourIndex?.Position ?? dayIndex?.Position ?? monthIndex?.Position ?? yearIndex?.Position ?? 0;
        }

        private class YearIndex
        {
            public int Year { get; set; }
            public long Position { get; set; }
            public List<MonthIndex> MonthIndices { get; set; } = new List<MonthIndex>();

            public class MonthIndex
            {
                public int Month { get; set; }
                public long Position { get; set; }
                public List<DayIndex> DayIndices { get; set; } = new List<DayIndex>();

                public class DayIndex
                {
                    public int Day { get; set; }
                    public long Position { get; set; }
                    public List<HourIndex> HourIndices { get; set; } = new List<HourIndex>();

                    public class HourIndex
                    {
                        public int Hour { get; set; }
                        public long Position { get; set; }
                    }
                }
            }
        }
    }
}