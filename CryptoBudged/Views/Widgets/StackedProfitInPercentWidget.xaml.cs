using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for StackedProfitInPercentWidget.xaml
    /// </summary>
    public partial class StackedProfitInPercentWidget : UserControl
    {
        public StackedProfitInPercentWidget()
        {
            InitializeComponent();
            DataContext = new StackedProfitInPercentWidgetViewModel();
        }
    }
}
