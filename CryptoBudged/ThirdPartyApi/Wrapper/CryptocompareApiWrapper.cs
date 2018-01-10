using System;
using System.Linq;
using System.Net.Http;
using CryptoBudged.Models;
using NtFreX.RestClient.NET;

namespace CryptoBudged.ThirdPartyApi.Wrapper
{
    public class CryptocompareApiWrapper : IDisposable
    {
        public RestClient RestClient { get; }

        public event EventHandler RateLimitRaised;

        public CryptocompareApiWrapper()
        {
            int[] statusCodesToRetry = { 500, 520 };
            var retryStrategy = new RetryStrategy(
                maxTries: 3,
                retryWhenResult: message => statusCodesToRetry.Contains((int)message.StatusCode),
                retryWhenException: exception => true);

            RestClient = new RestClientBuilder()
                .WithHttpClient(new HttpClient())
                .HandleRateLimit(419, 5000)
                .AddEndpoint(CryptocompareApiEndpointNames.CurrentPrices, builder => builder
                    .BaseUri(args => $"https://min-api.cryptocompare.com/data/price?fsym={((CurrencyModel)args[0]).ShortName}&tsyms=BTC,USD,EUR,CHF,ETH")
                    .Retry(retryStrategy))
                .Build();

            RestClient.RateLimitRaised += (sender, args) => Console.WriteLine($"{GetType().Name} Rate limit raised!!!!!!!"); //RateLimitRaised?.Invoke(sender, args);
            RestClient.AfterEndpointCalled += (sender, s) => Console.WriteLine($"{GetType().Name} - {DateTime.Now}: {s.EndpointName}({string.Join(",", s.Arguments)}) => {s.Result}");
        }

        public void Dispose()
        {
            RestClient?.HttpClient?.Dispose();
        }

        public static class CryptocompareApiEndpointNames
        {
            public static string CurrentPrices { get; } = nameof(CurrentPrices);
        }
    }
}
