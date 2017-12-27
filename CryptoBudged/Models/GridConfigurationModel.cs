using System.ComponentModel;

namespace CryptoBudged.Models
{
    public class GridConfigurationModel
    {
        public string OrderBy { get; set; }
        public string HeaderName { get; set; }
        public ListSortDirection OrderDirection { get; set; }
    }
}
