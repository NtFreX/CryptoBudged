using System.Windows.Controls;

namespace CryptoBudged.Views
{
    /// <summary>
    /// Interaction logic for DepositWithdrawlDialog.xaml
    /// </summary>
    public partial class DepositWithdrawlDialog : UserControl
    {
        public DepositWithdrawlDialog()
        {
            InitializeComponent();
            DataContext = new DepositWithdrawlDialogViewModel();
        }
    }
}
