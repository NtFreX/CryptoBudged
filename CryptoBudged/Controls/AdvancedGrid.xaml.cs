using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CryptoBudged.Models;
using CryptoBudged.Services;
using Prism.Commands;

namespace CryptoBudged.Controls
{
    /// <summary>
    /// Interaction logic for AdvancedGrid.xaml
    /// </summary>
    public partial class AdvancedGrid : UserControl
    {
        private string _lastClickedHeader;
        private ListSortDirection _lastSortDirection;
        private SortDescriptionCollection _currentSortDescriptions = new SortDescriptionCollection();

        public string GridConfigurationName
        {
            get => (string) GetValue(GridConfigurationNameProperty);
            set => SetValue(GridConfigurationNameProperty, value);
        }

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

        public DelegateCommand<object> ItemClickedCommand
        {
            get => (DelegateCommand<object>)GetValue(ItemClickedCommandProperty);
            set => SetValue(ItemClickedCommandProperty, value);
        }

        public static readonly DependencyProperty ItemClickedCommandProperty =
            DependencyProperty.Register(nameof(ItemClickedCommand), typeof(DelegateCommand<object>), typeof(AdvancedGrid));

        public static readonly DependencyProperty ViewProperty = 
            DependencyProperty.Register(nameof(View), typeof(ViewBase), typeof(AdvancedGrid));

        public static readonly DependencyProperty ItemsSourceProperty = 
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AdvancedGrid));
        
        public static readonly DependencyProperty GridConfigurationNameProperty =
            DependencyProperty.Register(nameof(GridConfigurationName), typeof(string), typeof(AdvancedGrid));
        
        public AdvancedGrid()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == nameof(ItemsSource))
            {
                //https://stackoverflow.com/questions/11177351/wpf-datagrid-ignores-sortdescription
                foreach (var description in _currentSortDescriptions)
                {
                    ListView.Items.SortDescriptions.Add(description);
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!(ListView.View is GridView gridView))
                return;

            var configuration = GridConfigurationService.Instance.LoadConfiguration(GridConfigurationName);
            if (configuration != null)
            {
                var column = gridView.Columns.First(x => x.Header.ToString() == configuration.HeaderName);

                Sort(gridView, column, configuration.OrderBy, configuration.OrderDirection);
            }
        }
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is GridViewColumnHeader headerClicked))
                return;

            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
                return;

            if (!(ListView.View is GridView gridView))
                return;

            var columnIndex = gridView.Columns.IndexOf(headerClicked.Column);
            var column = gridView.Columns[columnIndex];

            var propertyToOrder = GetPropertyNameToOrder(column);
            if (string.IsNullOrEmpty(propertyToOrder))
                return;

            var sortDirection = ListSortDirection.Ascending;
            if (_lastClickedHeader == column.Header.ToString())
            {
                sortDirection = _lastSortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            Sort(gridView, column, propertyToOrder, sortDirection);
        }

        private string GetPropertyNameToOrder(GridViewColumn column)
        {
            var binding = column.DisplayMemberBinding as Binding;
            var bindingPath = binding?.Path?.Path;

            if (string.IsNullOrEmpty(bindingPath))
            {
                if (!(column is AdvancedGridViewColumn advancedColumn))
                    return string.Empty;

                bindingPath = advancedColumn.SortByPropertyName;

                if (string.IsNullOrEmpty(bindingPath))
                    return string.Empty;
            }

            return bindingPath;
        }
        private void Sort(GridView gridView, GridViewColumn column, string sortBy, ListSortDirection sortDirection)
        {
            var dataView = CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            if (dataView == null)
                return;

            dataView.SortDescriptions.Clear();

            var sortDescription = new SortDescription(sortBy, sortDirection);

            _currentSortDescriptions = new SortDescriptionCollection {sortDescription};

            foreach (var description in _currentSortDescriptions)
            {
                dataView.SortDescriptions.Add(description);
            }
            dataView.Refresh();
            
            if (!string.IsNullOrEmpty(_lastClickedHeader))
            {
                var lastColumn = gridView.Columns.First(x => x.Header.ToString() == _lastClickedHeader);
                lastColumn.HeaderTemplate = null;
            }

            var templateName = sortDirection == ListSortDirection.Ascending
                ? "HeaderTemplateArrowUp"
                : "HeaderTemplateArrowDown";

            column.HeaderTemplate = Resources[templateName] as DataTemplate;

            GridConfigurationService.Instance.SaveConfiguration(
                GridConfigurationName,
                new GridConfigurationModel
                {
                    OrderBy = sortBy,
                    OrderDirection = sortDirection,
                    HeaderName = column.Header.ToString()
                });

            _lastClickedHeader = column.Header.ToString();
            _lastSortDirection = sortDirection;
        }

        private void HandlePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemClickedCommand == null)
                return;

            if (!ItemClickedCommand.CanExecute(e))
                return;

            ItemClickedCommand.Execute(sender);
        }
    }
}
