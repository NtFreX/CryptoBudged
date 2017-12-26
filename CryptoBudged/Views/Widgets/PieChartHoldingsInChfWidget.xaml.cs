using System.Windows.Controls;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for PieChartHoldingsInChf.xaml
    /// </summary>
    public partial class PieChartHoldingsInChfWidget : UserControl
    {
        public PieChartHoldingsInChfWidget()
        {
            InitializeComponent();
            DataContext = new PieChartHoldingsInChfWidgetViewModel();
        }
    }
}
