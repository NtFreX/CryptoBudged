using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBudged.Factories;
using CryptoBudged.Models;

namespace CryptoBudged.Services.CurrencyExchange
{
    public class CurrencyExchangeFactory
    {
        private readonly List<ICurrencyExchangeService> _currencyExchangeServices = new List<ICurrencyExchangeService>(new ICurrencyExchangeService[]
        {
            new BitfinexCurrencyExchangeService(),
            new BinanceCurrencyExchangeService(), 
            new CoinbaseCurrencyExchangeService(), 
            //new LocalCurrencyExchangeService(),
            new CloudFiatCurrencyExchangeService()
        });

        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync(Action<double> progressCallback)
        {
            var progressIndex = 0;
            var serviceCount = _currencyExchangeServices.Count;
            void ProgressCallback(double progress)
            {
                progressCallback(progress / serviceCount + (100.0 / serviceCount * progressIndex));
            }

            progressCallback(0);
            foreach (var currencyExchangeService in _currencyExchangeServices)
            {
                if (!currencyExchangeService.IsInitialized)
                    await currencyExchangeService.InitializeAsync(ProgressCallback);

                progressIndex++;

                progressCallback(100.0 / serviceCount * progressIndex);
            }

            IsInitialized = true;
        }

        private CurrencyExchangeFactory() { }

        public async Task<bool> CanConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency)
        {
            foreach (var currencyExchangeService in _currencyExchangeServices)
            {
                if (await currencyExchangeService.CanConvertAsync(originCurrency, targetCurrency))
                    return true;
            }
            return false;
        }

        public async Task<double> ConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime)
        {
            if(!await CanConvertAsync(originCurrency, targetCurrency))
                throw new ArgumentException();
            if(!IsInitialized)
                throw new ArgumentException();

            var avaiableServices = new List<(ICurrencyExchangeService Service, TimeSpan RateLimit)>();
            foreach (var service in _currencyExchangeServices)
            {
                if (await service.CanConvertAsync(originCurrency, targetCurrency))
                    avaiableServices.Add((service, await service.IsRateLimitedAsync(originCurrency, targetCurrency, dateTime)));
            }

            //TODO: ignore rate limit and use some kind of prefered platform
            var bestMatch = avaiableServices.OrderBy(x => x.RateLimit.Ticks).First();
            return await bestMatch.Service.ConvertAsync(originCurrency, targetCurrency, amount, dateTime);
        }

        public async Task<double> TryConvertAsync(CurrencyModel originCurrency, CurrencyModel targetCurrency, double amount, DateTime dateTime)
        {
            if (originCurrency == targetCurrency)
            {
                return amount;
            }
            if (await CanConvertAsync(originCurrency, targetCurrency))
            {
                return await ConvertAsync(originCurrency, targetCurrency, amount, dateTime);
            }

            var usdCurrency = CurrencyFactory.Instance.GetByShortName("USD");
            if (await CanConvertAsync(originCurrency, usdCurrency))
            {
                var inUsd = await ConvertAsync(originCurrency, usdCurrency, amount, dateTime);
                if (await CanConvertAsync(usdCurrency, targetCurrency))
                {
                    return await ConvertAsync(usdCurrency, targetCurrency, inUsd, dateTime);
                }
            }

            var btcCurrency = CurrencyFactory.Instance.GetByShortName("BTC");
            if (await CanConvertAsync(originCurrency, btcCurrency))
            {
                var inBtc = await ConvertAsync(originCurrency, btcCurrency, amount, dateTime);
                if (await CanConvertAsync(btcCurrency, targetCurrency))
                {
                    return await ConvertAsync(btcCurrency, targetCurrency, inBtc, dateTime);
                }
                if (await CanConvertAsync(btcCurrency, usdCurrency))
                {
                    var inUsd = await ConvertAsync(btcCurrency, usdCurrency, amount, dateTime);
                    if (await CanConvertAsync(usdCurrency, targetCurrency))
                    {
                        return await ConvertAsync(usdCurrency, targetCurrency, inUsd, dateTime);
                    }
                }
            }

            var ethCurrency = CurrencyFactory.Instance.GetByShortName("ETH");
            if (await CanConvertAsync(originCurrency, ethCurrency))
            {
                var inEth = await ConvertAsync(originCurrency, ethCurrency, amount, dateTime);
                if (await CanConvertAsync(ethCurrency, targetCurrency))
                {
                    return await ConvertAsync(ethCurrency, targetCurrency, inEth, dateTime);
                }
                if (await CanConvertAsync(ethCurrency, usdCurrency))
                {
                    var inUsd = await ConvertAsync(ethCurrency, usdCurrency, amount, dateTime);
                    if (await CanConvertAsync(usdCurrency, targetCurrency))
                    {
                        return await ConvertAsync(usdCurrency, targetCurrency, inUsd, dateTime);
                    }
                }
            }

            throw new ArgumentException();
        }

        public static CurrencyExchangeFactory Instance { get; } = new CurrencyExchangeFactory();
    }
}
