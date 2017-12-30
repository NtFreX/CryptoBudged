using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptoBudged.Services
{
    public class HttpCachingService
    {
        private readonly Dictionary<string, (DateTime ExecutedOn, string Result)> _lastRequests = new Dictionary<string, (DateTime, string)>();
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly object _lockObj = new object();

        private HttpCachingService() { }

        public async Task<string> GetStringAsync(string url, TimeSpan cacheValidation)
        {
            lock (_lockObj)
            {
                if (_lastRequests.ContainsKey(url))
                {
                    if (cacheValidation == TimeSpan.MaxValue || _lastRequests[url].ExecutedOn >= DateTime.Now - cacheValidation)
                    {
                        return _lastRequests[url].Result;
                    }

                    _lastRequests.Remove(url);
                }
            }

            Console.WriteLine("Get - " + url);
            var result = await _httpClient.GetStringAsync(url);
            lock (_lockObj)
            {
                if (!_lastRequests.ContainsKey(url))
                {
                    _lastRequests.Add(url, (DateTime.Now, result));
                }
            }
            return result;
        }

        public static HttpCachingService Instance { get; } = new HttpCachingService();
    }
}
