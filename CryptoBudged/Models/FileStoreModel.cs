using System.Collections.Generic;

namespace CryptoBudged.Models
{
    public class FileStoreModel
    {
        public List<DepositWithdrawlModel> DepositWithdrawls { get; set; } = new List<DepositWithdrawlModel>();
        public List<ExchangeModel> Exchanges { get; set; } = new List<ExchangeModel>();
    }
}
