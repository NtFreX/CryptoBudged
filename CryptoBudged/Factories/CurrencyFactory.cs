using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.Factories
{
    public class CurrencyFactory
    {
        private readonly IList<CurrencyModel> _currencies = JsonConvert.DeserializeObject<List<CurrencyModel>>(File.ReadAllText(@"Seed\currency.json"));

        private CurrencyFactory() { }

        public IEnumerable<CurrencyModel> GetAll()
            => _currencies;
        public CurrencyModel GetByShortName(string shortName)
            => _currencies.FirstOrDefault(x => x.ShortName == shortName);

        public static CurrencyFactory Instance { get; } = new CurrencyFactory();
    }
}
