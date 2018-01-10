using System;
using System.Linq;
using System.Net.Http;
using CryptoBudged.Models;
using NtFreX.RestClient.NET;

namespace CryptoBudged.ThirdPartyApi.Wrapper
{
    public class FixerApiWrapper : IDisposable
    {
        public RestClient RestClient { get; }

        public event EventHandler RateLimitRaised;

        public FixerApiWrapper()
        {
            int[] statusCodesToRetry = { 500, 520 };
            var retryStrategy = new RetryStrategy(
                maxTries: 3,
                retryWhenResult: message => statusCodesToRetry.Contains((int)message.StatusCode),
                retryWhenException: exception => true);

            RestClient = new RestClientBuilder()
                .WithHttpClient(new HttpClient())
                .HandleRateLimit(419, 5000)
                .AddEndpoint(FixerApiEndpointNames.Trades, builder => builder
                    .BaseUri(args => $"https://api.fixer.io/{FormatDate((DateTime)args[0])}?base={((CurrencyModel)args[1]).ShortName}")
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

        public static class FixerApiEndpointNames
        {
            public static string Trades { get; } = nameof(Trades);
        }
    }
}
