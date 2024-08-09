using Examine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAthletic.core.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
//using Umbraco.Cms.Web.UI.Services;


namespace TheAthletic.core.Services
{
    public class SearchService : ISearchService
    {
        private readonly IExamineManager _examineManager;
        private readonly UmbracoHelper _umbracoHelper;

        public SearchService(IExamineManager examineManager, UmbracoHelper umbracoHelper)
        {
            _examineManager = examineManager;
            _umbracoHelper = umbracoHelper;
        }

        public IEnumerable<IPublishedContent> SearchContentNames(string query)
        {
            if (string.IsNullOrEmpty(query) || !_examineManager.TryGetIndex("ExternalIndex", out IIndex? index))
            {
                return Enumerable.Empty<IPublishedContent>();
            }

            var searchResults = index.Searcher
                .CreateQuery("content")
                .GroupedOr(new[] { "nodeName", "newsTitle", "blockListContent" }, query)
                .Execute();

            return searchResults
                .Select(x => _umbracoHelper.Content(x.Id))
                .Where(x => x != null);
        }
    }
}