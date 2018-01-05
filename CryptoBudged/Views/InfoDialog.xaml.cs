using System.Windows.Controls;

namespace CryptoBudged
{
    /// <summary>
    /// Interaction logic for InfoPage.xaml
    /// </summary>
    public partial class InfoDialog : UserControl
    {
        public InfoDialog()
        {
            InitializeComponent();
            DataContext = new InfoDialogViewModel();
        }
    }
}
