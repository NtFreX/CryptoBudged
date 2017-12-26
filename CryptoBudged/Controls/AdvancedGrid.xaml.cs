using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CryptoBudged.Controls
{
    /// <summary>
    /// Interaction logic for AdvancedGrid.xaml
    /// </summary>
    public partial class AdvancedGrid : UserControl
    {
        public ViewBase View
        {
            get => (ViewBase)GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ViewProperty = 
            DependencyProperty.Register(nameof(View), typeof(ViewBase), typeof(AdvancedGrid));

        public static readonly DependencyProperty ItemsSourceProperty = 
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AdvancedGrid));

        public AdvancedGrid()
        {
            InitializeComponent();
        }
    }
}
