using System.Windows.Controls;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for HoldingsInBtcWidget.xaml
    /// </summary>
    public partial class HoldingsInBtcWidget : UserControl
    {
        public HoldingsInBtcWidget()
        {
            InitializeComponent();
            DataContext = new HoldingsInBtcWidgetViewModel();
        }
    }
}
