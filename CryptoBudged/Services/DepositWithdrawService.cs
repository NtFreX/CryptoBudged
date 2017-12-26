using System.Collections.Generic;
using System.Linq;
using CryptoBudged.Models;

namespace CryptoBudged.Services
{
    public class DepositWithdrawService
    {
        private readonly List<DepositWithdrawlModel> _depositWithdrawls;

        private DepositWithdrawService()
        {
            if (FileStoreService.Instance.HasSaves())
            {
                var fileStore = FileStoreService.Instance.Load();
                _depositWithdrawls = fileStore.DepositWithdrawls;
            }
            else
            {
                _depositWithdrawls = new List<DepositWithdrawlModel>();
            }
        }

        public IList<DepositWithdrawlModel> GetAll()
            => _depositWithdrawls;

        public void Update(IList<DepositWithdrawlModel> depositWithdrawls)
        {
            var fileStore = new FileStoreModel
            {
                DepositWithdrawls = depositWithdrawls.ToList(),
                Exchanges = ExchangesService.Instance.GetAll().ToList()
            };

            FileStoreService.Instance.Save(fileStore);
        }

        public static DepositWithdrawService Instance { get; } = new DepositWithdrawService();
    }
}
