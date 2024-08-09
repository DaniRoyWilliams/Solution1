using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAthletic.core.Models.Account
{
    public class SearchResultViewModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string BodyText { get; set; }
        public string Excerpt { get; set; }
    }

    public class SearchViewModel
    {
        public string Query { get; set; }
        public IEnumerable<SearchResultViewModel> SearchResults { get; set; } = new List<SearchResultViewModel>();
        public bool HasSearched { get; set; }
    }
}
