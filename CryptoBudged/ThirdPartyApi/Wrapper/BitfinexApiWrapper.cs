using System;
using System.Linq;
using System.Net.Http;
using CryptoBudged.Extensions;
using NtFreX.RestClient.NET;

namespace CryptoBudged.ThirdPartyApi.Wrapper
{
    public class BitfinexApiWrapper : IDisposable
    {
        public RestClient RestClient { get; }

        public event EventHandler RateLimitRaised;

        public BitfinexApiWrapper()
        {
            int[] statusCodesToRetry = { 500, 520 };
            var retryStrategy = new RetryStrategy(
                maxTries: 3,
                retryWhenResult: message => statusCodesToRetry.Contains((int)message.StatusCode),
                retryWhenException: exception => true);

            RestClient = new RestClientBuilder()
                .WithHttpClient(new HttpClient())
                .HandleRateLimit(419, 5000)
                .AddEndpoint(BitfinexApiEndpointNames.Symbols, builder => builder
                    .BaseUri("https://api.bitfinex.com/v1/symbols")
                    .Cache(TimeSpan.FromHours(1))
                    .Retry(retryStrategy))
                .AddEndpoint(BitfinexApiEndpointNames.Trades, builder => builder
                    .BaseUri(args => $"https://api.bitfinex.com/v2/trades/t{args[0]}/hist")
                    .AddQueryStringParam((args, uri) => ("end", ((DateTime) args[1]).ToUnixTimeMilliseconds().ToString()))
                    .Cache(TimeSpan.MaxValue)
                    .Retry(retryStrategy))
                .Build();

            RestClient.RateLimitRaised += (sender, args) => Console.WriteLine($"{GetType().Name} Rate limit raised!!!!!!!"); //RateLimitRaised?.Invoke(sender, args);
            RestClient.AfterEndpointCalled += (sender, s) => Console.WriteLine($"{GetType().Name} - {DateTime.Now}: {s.EndpointName}({string.Join(",", s.Arguments)}) => {s.Result}");
        }

        public void Dispose()
        {
            RestClient?.HttpClient?.Dispose();
        }

        public static class BitfinexApiEndpointNames
        {
            public static string Symbols { get; } = nameof(Symbols);
            public static string Trades { get; } = nameof(Trades);
        }
    }
}
