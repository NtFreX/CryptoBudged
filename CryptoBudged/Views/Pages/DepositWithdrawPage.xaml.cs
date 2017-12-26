using System.Windows.Controls;

namespace CryptoBudged.Views.Pages
{
    /// <summary>
    /// Interaction logic for DepositWithdrawPage.xaml
    /// </summary>
    public partial class DepositWithdrawPage : UserControl
    {
        public DepositWithdrawPage()
        {
            InitializeComponent();
            DataContext = new DepositWithdrawViewModel();
        }
    }
}
