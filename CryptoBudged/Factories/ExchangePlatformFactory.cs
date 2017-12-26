using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.Factories
{
    public class ExchangePlatformFactory
    {
        private readonly IList<ExchangePlatformModel> _exchangePlatforms = JsonConvert.DeserializeObject<List<ExchangePlatformModel>>(File.ReadAllText(@"Seed\exchangeplatforms.json"));

        private ExchangePlatformFactory() { }

        public IEnumerable<ExchangePlatformModel> GetAll()
            => _exchangePlatforms;
        public IEnumerable<ExchangePlatformModel> GetCryptoCurrencyExchanges()
            => _exchangePlatforms.Where(x => x.ExchangesCryptoCurrencies);
        public IEnumerable<ExchangePlatformModel> GetAllTraditionalExchanges()
            => _exchangePlatforms.Where(x => !x.ExchangesCryptoCurrencies);

        public static ExchangePlatformFactory Instance { get; } = new ExchangePlatformFactory();
    }
}
