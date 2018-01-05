using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoBudged.Services
{
    //public class HttpCachingService
    //{
    //    private const string CacheFileStore = "web.cache";

    //    private readonly Dictionary<string, (DateTime ExecutedOn, string Result)> _lastRequests = new Dictionary<string, (DateTime, string)>();
    //    private readonly HttpClient _httpClient = new HttpClient();
    //    private readonly object _lockObj = new object();

    //    private HttpCachingService()
    //    {
    //        if (File.Exists(CacheFileStore))
    //        {
    //            _lastRequests = JsonConvert.DeserializeObject<Dictionary<string, (DateTime, string)>>(File.ReadAllText(CacheFileStore));

    //            if (_lastRequests == null)
    //            {
    //                _lastRequests = new Dictionary<string, (DateTime ExecutedOn, string Result)>();
    //                File.Delete(CacheFileStore);
    //            }
    //        }

    //    }

    //    public async Task<string> GetStringAsync(string url, TimeSpan cacheValidation)
    //    {
    //        lock (_lockObj)
    //        {
    //            if (_lastRequests.ContainsKey(url))
    //            {
    //                if (cacheValidation == TimeSpan.MaxValue || _lastRequests[url].ExecutedOn >= DateTime.Now - cacheValidation)
    //                {
    //                    return _lastRequests[url].Result;
    //                }

    //                _lastRequests.Remove(url);
    //                UpdateCacheStore();
    //            }
    //        }
            
    //        Console.WriteLine($"{DateTime.Now} - HttpCachingService.GetStringAsync - {url}");

    //        var result = await _httpClient.GetStringAsync(url);
    //        lock (_lockObj)
    //        {
    //            if (!_lastRequests.ContainsKey(url))
    //            {
    //                _lastRequests.Add(url, (DateTime.Now, result));
    //                UpdateCacheStore();
    //            }
    //        }
    //        return result;
    //    }

    //    public bool HasCached(string url, TimeSpan cacheValidation)
    //    {
    //        lock (_lockObj)
    //        {
    //            if (_lastRequests.ContainsKey(url))
    //            {
    //                if (cacheValidation == TimeSpan.MaxValue ||
    //                    _lastRequests[url].ExecutedOn >= DateTime.Now - cacheValidation)
    //                {
    //                    return true;
    //                }
    //            }
    //            return false;
    //        }
    //    }

    //    private void UpdateCacheStore()
    //    {
    //        File.WriteAllText(CacheFileStore, JsonConvert.SerializeObject(_lastRequests));
    //    }

    //    public static HttpCachingService Instance { get; } = new HttpCachingService();
    //}
}
