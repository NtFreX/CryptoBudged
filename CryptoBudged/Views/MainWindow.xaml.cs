using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace CryptoBudged.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GridViewColumnHeader _lastClickedExchangeHeader;
        private GridViewColumnHeader _lastClickedDepositWithdrawHeader;
        private GridViewColumnHeader _lastClickedHoldingHeader;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }

        private void ExchangeHeaderColumnClicked(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;

            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked == null)
                return;
            if (viewModel == null)
                return;
            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
                return;

            var direction = viewModel.SortExchanges(headerClicked);
            
            if (direction == ListSortDirection.Ascending)
            {
                headerClicked.Column.HeaderTemplate =
                    Resources["HeaderTemplateArrowUp"] as DataTemplate;
            }
            else
            {
                headerClicked.Column.HeaderTemplate =
                    Resources["HeaderTemplateArrowDown"] as DataTemplate;
            }

            // Remove arrow from previously sorted header  
            if (_lastClickedExchangeHeader != null && _lastClickedExchangeHeader != headerClicked)
            {
                _lastClickedExchangeHeader.Column.HeaderTemplate = null;
            }

            _lastClickedExchangeHeader = headerClicked;
        }

        private void DepositWithdrawlHeaderColumnClicked(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;

            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked == null)
                return;
            if (viewModel == null)
                return;
            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
                return;

            var direction = viewModel.SortDepositsWithdrawls(headerClicked);

            if (direction == ListSortDirection.Ascending)
            {
                headerClicked.Column.HeaderTemplate =
                    Resources["HeaderTemplateArrowUp"] as DataTemplate;
            }
            else
            {
                headerClicked.Column.HeaderTemplate =
                    Resources["HeaderTemplateArrowDown"] as DataTemplate;
            }

            // Remove arrow from previously sorted header  
            if (_lastClickedDepositWithdrawHeader != null && _lastClickedDepositWithdrawHeader != headerClicked)
            {
                _lastClickedDepositWithdrawHeader.Column.HeaderTemplate = null;
            }

            _lastClickedDepositWithdrawHeader = headerClicked;
        }

        private void HoldingsHeaderColumnClicked(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;

            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked == null)
                return;
            if (viewModel == null)
                return;
            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
                return;

            var direction = viewModel.SortHolding(headerClicked);

            if (direction == ListSortDirection.Ascending)
            {
                headerClicked.Column.HeaderTemplate =
                    Resources["HeaderTemplateArrowUp"] as DataTemplate;
            }
            else
            {
                headerClicked.Column.HeaderTemplate =
                    Resources["HeaderTemplateArrowDown"] as DataTemplate;
            }

            // Remove arrow from previously sorted header  
            if (_lastClickedHoldingHeader != null && _lastClickedHoldingHeader != headerClicked)
            {
                _lastClickedHoldingHeader.Column.HeaderTemplate = null;
            }

            _lastClickedHoldingHeader = headerClicked;
        }
    }
}
