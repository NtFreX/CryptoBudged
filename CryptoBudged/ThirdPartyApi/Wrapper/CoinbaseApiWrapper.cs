using System;
using System.Linq;
using System.Net.Http;
using CryptoBudged.Models;
using NtFreX.RestClient.NET;

namespace CryptoBudged.ThirdPartyApi.Wrapper
{
    public class CoinbaseApiWrapper : IDisposable
    {
        public RestClient RestClient { get; }

        public event EventHandler RateLimitRaised;

        public CoinbaseApiWrapper()
        {
            int[] statusCodesToRetry = { 500, 520 };
            var retryStrategy = new RetryStrategy(
                maxTries: 3,
                retryWhenResult: message => statusCodesToRetry.Contains((int)message.StatusCode),
                retryWhenException: exception => true);

            RestClient = new RestClientBuilder()
                .WithHttpClient(new HttpClient())
                .HandleRateLimit(419, 5000)
                .AddEndpoint(CoinbaseApiEndpointNames.Currencies, builder => builder
                    .BaseUri("https://api.coinbase.com/v2/currencies")
                    .Cache(TimeSpan.FromHours(1))
                    .Retry(retryStrategy))
                .AddEndpoint(CoinbaseApiEndpointNames.Prices, builder => builder
                    .BaseUri(args => $"https://api.coinbase.com/v2/prices/{((CurrencyModel)args[0]).ShortName}-{((CurrencyModel)args[1]).ShortName}/spot")
                    .AddQueryStringParam((args, uri) => ("date", FormatDate((DateTime)args[2])))
                    .Cache(TimeSpan.MaxValue)
                    .Retry(retryStrategy))
                .Build();

            RestClient.RateLimitRaised += (sender, args) => Console.WriteLine($"{GetType().Name} Rate limit raised!!!!!!!"); //RateLimitRaised?.Invoke(sender, args);
            RestClient.AfterEndpointCalled += (sender, s) => Console.WriteLine($"{GetType().Name} - {DateTime.Now}: {s.EndpointName}({string.Join(",", s.Arguments)}) => {s.Result}");
        }

        #region Helpers
        private static string FormatDate(DateTime dateTime)
            => $"{dateTime.Year:0000}-{dateTime.Month:00}-{dateTime.Day:00}";
        #endregion

        public void Dispose()
        {
            RestClient?.HttpClient?.Dispose();
        }

        public static class CoinbaseApiEndpointNames
        {
            public static string Currencies { get; } = nameof(Currencies);
            public static string Prices { get; } = nameof(Prices);
        }
    }
}
