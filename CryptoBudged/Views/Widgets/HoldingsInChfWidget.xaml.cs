using System.Windows.Controls;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for HoldingsInChfWidget.xaml
    /// </summary>
    public partial class HoldingsInChfWidget : UserControl
    {
        public HoldingsInChfWidget()
        {
            InitializeComponent();
            DataContext = new HoldingsInChfWidgetViewModel();
        }
    }
}
