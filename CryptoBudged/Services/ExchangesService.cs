using System.Collections.Generic;
using System.Linq;
using CryptoBudged.Models;

namespace CryptoBudged.Services
{
    public class ExchangesService
    {
        private readonly List<ExchangeModel> _exchanges;

        private ExchangesService()
        {
            if (FileStoreService.Instance.HasSaves())
            {
                var fileStore = FileStoreService.Instance.Load();
                _exchanges = fileStore.Exchanges;
            }
            else
            {
                _exchanges = new List<ExchangeModel>();
            }
        }

        public IList<ExchangeModel> GetAll()
            => _exchanges;

        public void Update(IList<ExchangeModel> exchanges)
        {
            var fileStore = new FileStoreModel
            {
                DepositWithdrawls = DepositWithdrawService.Instance.GetAll().ToList(),
                Exchanges = exchanges.ToList()
            };

            FileStoreService.Instance.Save(fileStore);
        }

        public static ExchangesService Instance { get; } = new ExchangesService();
    }
}
