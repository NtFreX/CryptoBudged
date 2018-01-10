using System;
using System.Threading.Tasks;
using CryptoBudged.Models;
using CryptoBudged.ThirdPartyApi.Wrapper;
using Newtonsoft.Json;
using NtFreX.RestClient.NET.Flow;

namespace CryptoBudged.ThirdPartyApi
{
    public class CryptocompareApi : IDisposable
    {
        private readonly CryptocompareApiWrapper _cryptocompareApiWrapper = new CryptocompareApiWrapper();

        private CryptocompareApi()
        {
            GetCurrentPrices = new AsyncCachedFunction<CurrencyModel, PriceModel>(GetCurrentPricesAsync, TimeSpan.FromMinutes(10));
        }

        #region Public

        public readonly AsyncCachedFunction<CurrencyModel, PriceModel> GetCurrentPrices;
        private async Task<PriceModel> GetCurrentPricesAsync(CurrencyModel currency)
        {
            var content = await _cryptocompareApiWrapper.RestClient.CallEndpointAsync(CryptocompareApiWrapper.CryptocompareApiEndpointNames.CurrentPrices, currency);
            return JsonConvert.DeserializeObject<PriceModel>(content);
        }
        #endregion

        public static CryptocompareApi Instance { get; } = new CryptocompareApi();

        public void Dispose()
        {
            _cryptocompareApiWrapper?.Dispose();
        }
    }
}
