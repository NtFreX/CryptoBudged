using System.Windows.Controls;

namespace CryptoBudged.Views.Pages
{
    /// <summary>
    /// Interaction logic for ExchangesPage.xaml
    /// </summary>
    public partial class ExchangesPage : UserControl
    {
        public ExchangesPage()
        {
            InitializeComponent();

            DataContext = new ExchangesPageViewModel();
        }
    }
}
