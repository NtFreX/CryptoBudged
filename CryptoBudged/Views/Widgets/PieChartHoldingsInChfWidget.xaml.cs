using System.Windows.Controls;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for PieChartHoldingsInChf.xaml
    /// </summary>
    public partial class PieChartHoldingsInChf : UserControl
    {
        public PieChartHoldingsInChf()
        {
            InitializeComponent();
            DataContext = new PieChartHoldingsInChfWidgetViewModel();
        }
    }
}
