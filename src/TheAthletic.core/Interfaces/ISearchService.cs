using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TheAthletic.core.Interfaces
{
    public interface ISearchService
    {
        IEnumerable<IPublishedContent> SearchContentNames(string query);
    }
}
