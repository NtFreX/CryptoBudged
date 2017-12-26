using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace CryptoBudged.Views
{
    /// <summary>
    /// Interaction logic for ExchangeDialog.xaml
    /// </summary>
    public partial class ExchangeDialog : UserControl
    {
        public ExchangeDialog()
        {
            InitializeComponent();

            DataContext = new ExchangeDialogViewModel();
        }
    }
}
