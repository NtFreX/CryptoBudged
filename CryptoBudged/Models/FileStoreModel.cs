using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBudged.Models
{
    public class FileStoreModel
    {
        public List<DepositWithdrawlModel> DepositWithdrawls { get; set; } = new List<DepositWithdrawlModel>();
        public List<ExchangeModel> Exchanges { get; set; } = new List<ExchangeModel>();
    }
}
