using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CryptoBudged.Controls
{
    public class AdvancedGridViewColumn : GridViewColumn
    {
        public string SortByPropertyName
        {
            get => (string)GetValue(SortByPropertyNameProperty);
            set => SetValue(SortByPropertyNameProperty, value);
        }
        
        public static readonly DependencyProperty SortByPropertyNameProperty =
            DependencyProperty.Register(nameof(SortByPropertyName), typeof(string), typeof(AdvancedGridViewColumn));
    }
}