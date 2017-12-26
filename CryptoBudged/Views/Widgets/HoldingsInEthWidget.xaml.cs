using System.Windows.Controls;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for HoldingsInEthWidget.xaml
    /// </summary>
    public partial class HoldingsInEthWidget : UserControl
    {
        public HoldingsInEthWidget()
        {
            InitializeComponent();
            DataContext = new HoldingsInEthWidgetViewModel();
        }
    }
}
