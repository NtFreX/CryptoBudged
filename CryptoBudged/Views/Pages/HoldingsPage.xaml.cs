using System.Windows.Controls;

namespace CryptoBudged.Views.Pages
{
    /// <summary>
    /// Interaction logic for HoldingsPage.xaml
    /// </summary>
    public partial class HoldingsPage : UserControl
    {
        public HoldingsPage()
        {
            InitializeComponent();
            DataContext = new HoldingsViewModel();
        }
    }
}
